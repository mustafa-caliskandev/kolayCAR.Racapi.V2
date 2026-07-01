param(
    [string]$AirportsPath = (Join-Path $PSScriptRoot "..\airports.txt"),
    [string]$OutputPath = (Join-Path $PSScriptRoot "pandora2-locationvendor-import.sql"),
    [int]$VendorId = 100,
    [int]$PreferredLocationLangId = 1
)

$ErrorActionPreference = "Stop"

function ConvertTo-SqlString {
    param([object]$Value)

    $text = [string]$Value
    if ([string]::IsNullOrWhiteSpace($text)) {
        return "NULL"
    }

    return "N'$($text.Replace("'", "''"))'"
}

if (!(Test-Path -LiteralPath $AirportsPath)) {
    throw "Airports file not found: $AirportsPath"
}

$airportsJson = [System.IO.File]::ReadAllText((Resolve-Path -LiteralPath $AirportsPath).Path, [System.Text.Encoding]::UTF8)
$airports = $airportsJson | ConvertFrom-Json
$validAirports = @(
    $airports |
        Where-Object {
            $_.LocationId -gt 0 -and
            ![string]::IsNullOrWhiteSpace($_.LocationCode) -and
            ![string]::IsNullOrWhiteSpace($_.IataCode) -and
            ![string]::IsNullOrWhiteSpace($_.LocationName)
        } |
        Sort-Object IataCode, CountryCode, LocationName, LocationCode
)

$values = New-Object System.Collections.Generic.List[string]
for ($i = 0; $i -lt $validAirports.Count; $i++) {
    $airport = $validAirports[$i]
    $values.Add("($($i + 1), $($airport.LocationId), $(ConvertTo-SqlString $airport.LocationCode), $(ConvertTo-SqlString $airport.CountryCode), $(ConvertTo-SqlString $airport.IataCode), $(ConvertTo-SqlString $airport.LocationName))")
}

$airportInsertStatements = New-Object System.Collections.Generic.List[string]
$batchSize = 900
for ($offset = 0; $offset -lt $values.Count; $offset += $batchSize) {
    $count = [Math]::Min($batchSize, $values.Count - $offset)
    $batchValues = $values.GetRange($offset, $count)
    $airportInsertStatements.Add(@"
INSERT INTO @Airports (SourceOrder, ApiLocationId, ApiLocationCode, CountryCode, Iata, ApiLocationName)
VALUES
$($batchValues -join ",`r`n");
"@)
}
$airportInsertSql = $airportInsertStatements -join "`r`n`r`n"

$generatedAt = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$sql = @"
-- Pandora2 LOCATIONVENDOR import preview/insert script
-- Generated at: $generatedAt
-- Source: $AirportsPath
--
-- Usage:
-- 1. Run as-is with @DoInsert = 0 and inspect report result sets.
-- 2. Fill @CountryOverrides if duplicate IATA choices should change.
-- 3. Set @DoInsert = 1 only after preview looks correct.

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @DoInsert bit = 0;
DECLARE @VendorId int = $VendorId;
DECLARE @PreferredLocationLangId int = $PreferredLocationLangId;

DECLARE @CountryOverrides TABLE (
    Iata nvarchar(10) NOT NULL PRIMARY KEY,
    CountryCode nvarchar(2) NOT NULL
);

-- Same IATA can appear more than once in Pandora2.
-- Example:
-- INSERT INTO @CountryOverrides (Iata, CountryCode) VALUES (N'GVA', N'CH');

DECLARE @Airports TABLE (
    SourceOrder int NOT NULL,
    ApiLocationId int NOT NULL,
    ApiLocationCode nvarchar(150) NOT NULL,
    CountryCode nvarchar(2) NULL,
    Iata nvarchar(10) NOT NULL,
    ApiLocationName nvarchar(250) NOT NULL
);

$airportInsertSql

DECLARE @Resolved TABLE (
    SourceOrder int NOT NULL,
    ApiLocationId int NOT NULL,
    ApiLocationCode nvarchar(150) NOT NULL,
    CountryCode nvarchar(2) NULL,
    Iata nvarchar(10) NOT NULL,
    ApiLocationName nvarchar(250) NOT NULL,
    DuplicateRank int NOT NULL,
    LocalLocationId int NULL,
    LocalLocationName nvarchar(200) NULL,
    ExistingLocationVendorId int NULL
);

;WITH RankedAirports AS (
    SELECT
        a.SourceOrder,
        a.ApiLocationId,
        a.ApiLocationCode,
        COALESCE(co.CountryCode, a.CountryCode) AS CountryCode,
        UPPER(LTRIM(RTRIM(a.Iata))) AS Iata,
        a.ApiLocationName,
        ROW_NUMBER() OVER (
            PARTITION BY UPPER(LTRIM(RTRIM(a.Iata)))
            ORDER BY
                CASE WHEN co.Iata IS NOT NULL AND co.CountryCode = a.CountryCode THEN 0 ELSE 1 END,
                a.SourceOrder
        ) AS DuplicateRank
    FROM @Airports a
    LEFT JOIN @CountryOverrides co ON co.Iata = UPPER(LTRIM(RTRIM(a.Iata)))
)
INSERT INTO @Resolved (
    SourceOrder,
    ApiLocationId,
    ApiLocationCode,
    CountryCode,
    Iata,
    ApiLocationName,
    DuplicateRank,
    LocalLocationId,
    LocalLocationName,
    ExistingLocationVendorId
)
SELECT
    a.SourceOrder,
    a.ApiLocationId,
    a.ApiLocationCode,
    a.CountryCode,
    a.Iata,
    a.ApiLocationName,
    a.DuplicateRank,
    localLocation.ID AS LocalLocationId,
    localLocation.LOCATIONNAME AS LocalLocationName,
    existingLocationVendor.ID AS ExistingLocationVendorId
FROM RankedAirports a
OUTER APPLY (
    SELECT TOP (1)
        l.ID,
        l.LOCATIONNAME
    FROM LOCATION l
    WHERE UPPER(LTRIM(RTRIM(l.IATA))) = a.Iata
        AND ISNULL(l.ISDELETED, 0) = 0
    ORDER BY
        CASE WHEN l.LANGID = @PreferredLocationLangId THEN 0 ELSE 1 END,
        l.LANGID,
        l.ID
) localLocation
OUTER APPLY (
    SELECT TOP (1)
        lv.ID
    FROM LOCATIONVENDOR lv
    WHERE lv.VENDORID = @VendorId
        AND (
            lv.LOCALLOCATIONID = localLocation.ID
            OR lv.LOCATIONCODE = a.ApiLocationCode
            OR lv.LOCATIONID = a.ApiLocationId
        )
    ORDER BY lv.ID
) existingLocationVendor;

SELECT
    N'DuplicateIata' AS ReportType,
    Iata,
    COUNT(*) AS DuplicateCount,
    STRING_AGG(CONCAT(CountryCode, N': ', ApiLocationName, N' / ', ApiLocationCode), N' | ') AS Candidates
FROM @Resolved
GROUP BY Iata
HAVING COUNT(*) > 1
ORDER BY Iata;

SELECT
    N'MissingLocalLocation' AS ReportType,
    Iata,
    ApiLocationName,
    CountryCode,
    ApiLocationId,
    ApiLocationCode
FROM @Resolved
WHERE DuplicateRank = 1
    AND LocalLocationId IS NULL
ORDER BY Iata;

SELECT
    N'AlreadyMapped' AS ReportType,
    Iata,
    ExistingLocationVendorId,
    LocalLocationId,
    LocalLocationName,
    ApiLocationId,
    ApiLocationCode,
    ApiLocationName
FROM @Resolved
WHERE DuplicateRank = 1
    AND ExistingLocationVendorId IS NOT NULL
ORDER BY Iata;

SELECT
    N'WillInsert' AS ReportType,
    @VendorId AS VendorId,
    LocalLocationId,
    LocalLocationName,
    ApiLocationId,
    ApiLocationCode,
    ApiLocationName,
    Iata
FROM @Resolved
WHERE DuplicateRank = 1
    AND LocalLocationId IS NOT NULL
    AND ExistingLocationVendorId IS NULL
ORDER BY Iata;

IF @DoInsert = 1
BEGIN
    BEGIN TRANSACTION;

    INSERT INTO LOCATIONVENDOR (
        VENDORID,
        LOCALLOCATIONID,
        LOCATIONID,
        ACTIVE,
        ISPICKUP,
        APILOCATIONNAME,
        ISOFFICE,
        LOCATIONCODE,
        VENDORORDER,
        DISTRICTCODE,
        CITYCODE,
        FLIGHTCARDMANDATORY,
        EARLIESTRESTIME,
        RATECODE,
        ISFUELPOLICY
    )
    SELECT
        @VendorId,
        LocalLocationId,
        ApiLocationId,
        1,
        1,
        LEFT(ApiLocationName, 250),
        0,
        ApiLocationCode,
        NULL,
        NULL,
        NULL,
        NULL,
        NULL,
        NULL,
        NULL
    FROM @Resolved
    WHERE DuplicateRank = 1
        AND LocalLocationId IS NOT NULL
        AND ExistingLocationVendorId IS NULL
    ORDER BY Iata;

    SELECT @@ROWCOUNT AS InsertedCount;

    COMMIT TRANSACTION;
END
ELSE
BEGIN
    SELECT N'PreviewOnly' AS Status, COUNT(*) AS InsertCandidateCount
    FROM @Resolved
    WHERE DuplicateRank = 1
        AND LocalLocationId IS NOT NULL
        AND ExistingLocationVendorId IS NULL;
END
"@

Set-Content -LiteralPath $OutputPath -Value $sql -Encoding UTF8

[pscustomobject]@{
    SourceCount = $airports.Count
    ValidCount = $validAirports.Count
    VendorId = $VendorId
    OutputPath = (Resolve-Path -LiteralPath $OutputPath).Path
}
