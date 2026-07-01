param(
    [string]$AirportsPath = (Join-Path $PSScriptRoot "..\airports.txt"),
    [string]$OutputPath = (Join-Path $PSScriptRoot "pandora2-airport-import.sql"),
    [int[]]$LangIds = @(1, 2, 3),
    [int]$FallbackCityId = 1
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
            ![string]::IsNullOrWhiteSpace($_.IataCode) -and
            ![string]::IsNullOrWhiteSpace($_.CountryCode) -and
            ![string]::IsNullOrWhiteSpace($_.LocationName)
        } |
        Sort-Object IataCode, CountryCode, LocationName
)

$values = New-Object System.Collections.Generic.List[string]
for ($i = 0; $i -lt $validAirports.Count; $i++) {
    $airport = $validAirports[$i]
    $values.Add("($($i + 1), $(ConvertTo-SqlString $airport.CountryCode), $(ConvertTo-SqlString $airport.IataCode), $(ConvertTo-SqlString $airport.LocationName))")
}

$airportInsertStatements = New-Object System.Collections.Generic.List[string]
$batchSize = 900
for ($offset = 0; $offset -lt $values.Count; $offset += $batchSize) {
    $count = [Math]::Min($batchSize, $values.Count - $offset)
    $batchValues = $values.GetRange($offset, $count)
    $airportInsertStatements.Add(@"
INSERT INTO @Airports (SourceOrder, CountryCode, Iata, LocationName)
VALUES
$($batchValues -join ",`r`n");
"@)
}
$airportInsertSql = $airportInsertStatements -join "`r`n`r`n"
$langIdValues = @(
    $LangIds |
        Sort-Object -Unique |
        ForEach-Object {
            if ($_ -le 0) {
                throw "LangIds must contain positive values."
            }

            "($_)"
        }
) -join ",`r`n"

$generatedAt = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$sql = @"
-- Pandora2 airport import preview/insert script
-- Generated at: $generatedAt
-- Source: $AirportsPath
--
-- Usage:
-- 1. Run as-is with @DoInsert = 0 and inspect the report result sets.
-- 2. Fill @CountryOverrides or @CityOverrides if needed.
-- 3. Set @DoInsert = 1 only after the preview looks correct.

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @DoInsert bit = 0;
DECLARE @FallbackCityId int = $FallbackCityId;

DECLARE @LangIds TABLE (
    LangId int NOT NULL PRIMARY KEY
);

INSERT INTO @LangIds (LangId)
VALUES
$langIdValues;

DECLARE @CountryOverrides TABLE (
    Iata nvarchar(10) NOT NULL PRIMARY KEY,
    CountryCode nvarchar(2) NOT NULL
);

-- Same IATA can appear under multiple countries in Pandora2.
-- Example:
-- INSERT INTO @CountryOverrides (Iata, CountryCode) VALUES (N'GVA', N'CH');

DECLARE @CityOverrides TABLE (
    Iata nvarchar(10) NOT NULL PRIMARY KEY,
    CityId int NOT NULL
);

-- airports.txt has no separate city data.
-- The script first tries to match CITY by COUNTRYID + CITYNAME in LocationName.
-- Use this table for locations where automatic city matching is not correct.
-- Example:
-- INSERT INTO @CityOverrides (Iata, CityId) VALUES (N'AMS', 123);

DECLARE @Airports TABLE (
    SourceOrder int NOT NULL,
    CountryCode nvarchar(2) NOT NULL,
    Iata nvarchar(10) NOT NULL,
    LocationName nvarchar(200) NOT NULL
);

$airportInsertSql

DECLARE @Resolved TABLE (
    SourceOrder int NOT NULL,
    LangId int NOT NULL,
    CountryCode nvarchar(2) NOT NULL,
    Iata nvarchar(10) NOT NULL,
    LocationName nvarchar(200) NOT NULL,
    DuplicateRank int NOT NULL,
    LocalCountryId int NULL,
    CityId int NOT NULL,
    MatchedCityName nvarchar(100) NULL,
    CityMatchSource nvarchar(20) NOT NULL,
    ExistingLocationId int NULL,
    ExistingAnyLanguageLocationId int NULL
);

;WITH RankedAirports AS (
    SELECT
        a.SourceOrder,
        COALESCE(co.CountryCode, a.CountryCode) AS CountryCode,
        UPPER(LTRIM(RTRIM(a.Iata))) AS Iata,
        a.LocationName,
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
    LangId,
    CountryCode,
    Iata,
    LocationName,
    DuplicateRank,
    LocalCountryId,
    CityId,
    MatchedCityName,
    CityMatchSource,
    ExistingLocationId,
    ExistingAnyLanguageLocationId
)
SELECT
    a.SourceOrder,
    langs.LangId,
    a.CountryCode,
    a.Iata,
    a.LocationName,
    a.DuplicateRank,
    c.COUNTRYID AS LocalCountryId,
    COALESCE(cityOverride.CityId, matchedCity.CityId, @FallbackCityId) AS CityId,
    matchedCity.CityName AS MatchedCityName,
    CASE
        WHEN cityOverride.CityId IS NOT NULL THEN N'Override'
        WHEN matchedCity.CityId IS NOT NULL THEN N'CityName'
        ELSE N'Fallback'
    END AS CityMatchSource,
    existing.ID AS ExistingLocationId,
    existingAny.ID AS ExistingAnyLanguageLocationId
FROM RankedAirports a
CROSS JOIN @LangIds langs
LEFT JOIN COUNTRY c ON c.COUNTRYCODE2 = a.CountryCode
LEFT JOIN @CityOverrides cityOverride ON cityOverride.Iata = a.Iata
OUTER APPLY (
    SELECT TOP (1)
        ci.CITYID AS CityId,
        ci.CITYNAME AS CityName
    FROM CITY ci
    WHERE ci.COUNTRYID = c.COUNTRYID
        AND ci.CITYNAME IS NOT NULL
        AND LEN(LTRIM(RTRIM(ci.CITYNAME))) >= 3
        AND (
            a.LocationName COLLATE Latin1_General_CI_AI LIKE LTRIM(RTRIM(ci.CITYNAME)) COLLATE Latin1_General_CI_AI + N'%'
            OR a.LocationName COLLATE Latin1_General_CI_AI LIKE N'% ' + LTRIM(RTRIM(ci.CITYNAME)) COLLATE Latin1_General_CI_AI + N' %'
            OR a.LocationName COLLATE Latin1_General_CI_AI LIKE N'% ' + LTRIM(RTRIM(ci.CITYNAME)) COLLATE Latin1_General_CI_AI
        )
    ORDER BY
        CASE
            WHEN a.LocationName COLLATE Latin1_General_CI_AI LIKE LTRIM(RTRIM(ci.CITYNAME)) COLLATE Latin1_General_CI_AI + N'%' THEN 0
            ELSE 1
        END,
        LEN(LTRIM(RTRIM(ci.CITYNAME))) DESC,
        ci.CITYID
) matchedCity
LEFT JOIN LOCATION existing
    ON existing.LANGID = langs.LangId
    AND UPPER(LTRIM(RTRIM(existing.IATA))) = a.Iata
    AND ISNULL(existing.ISDELETED, 0) = 0
OUTER APPLY (
    SELECT TOP (1)
        locationInAnyLanguage.ID
    FROM LOCATION locationInAnyLanguage
    WHERE UPPER(LTRIM(RTRIM(locationInAnyLanguage.IATA))) = a.Iata
        AND ISNULL(locationInAnyLanguage.ISDELETED, 0) = 0
    ORDER BY locationInAnyLanguage.ID
) existingAny;

SELECT
    N'DuplicateIata' AS ReportType,
    Iata,
    COUNT(*) AS DuplicateCount,
    STRING_AGG(CONCAT(CountryCode, N': ', LocationName), N' | ') AS Candidates
FROM @Resolved
WHERE LangId = (SELECT MIN(LangId) FROM @LangIds)
GROUP BY Iata
HAVING COUNT(*) > 1
ORDER BY Iata;

SELECT
    N'MissingCountry' AS ReportType,
    CountryCode,
    COUNT(*) AS AirportCount
FROM @Resolved
WHERE LangId = (SELECT MIN(LangId) FROM @LangIds)
    AND DuplicateRank = 1
    AND LocalCountryId IS NULL
GROUP BY CountryCode
ORDER BY CountryCode;

SELECT
    N'AlreadyExists' AS ReportType,
    LangId,
    Iata,
    ExistingLocationId,
    ExistingAnyLanguageLocationId,
    LocationName,
    CountryCode,
    LocalCountryId
FROM @Resolved
WHERE DuplicateRank = 1 AND ExistingLocationId IS NOT NULL
ORDER BY Iata;

SELECT
    N'CityFallback' AS ReportType,
    LangId,
    Iata,
    LocationName,
    CountryCode,
    LocalCountryId,
    CityId
FROM @Resolved
WHERE DuplicateRank = 1
    AND LocalCountryId IS NOT NULL
    AND CityMatchSource = N'Fallback'
ORDER BY Iata;

SELECT
    N'WillInsert' AS ReportType,
    LangId,
    Iata,
    LocationName,
    CountryCode,
    LocalCountryId,
    CityId,
    CityMatchSource,
    MatchedCityName
FROM @Resolved
WHERE DuplicateRank = 1
    AND ExistingLocationId IS NULL
    AND LocalCountryId IS NOT NULL
ORDER BY Iata;

IF @DoInsert = 1
BEGIN
    BEGIN TRANSACTION;

    DECLARE @StartId int = (
        SELECT ISNULL(MAX(ID), 0)
        FROM LOCATION WITH (UPDLOCK, HOLDLOCK)
    );

    ;WITH InsertCandidates AS (
        SELECT
            r.*
        FROM @Resolved r
        WHERE r.DuplicateRank = 1
            AND r.ExistingLocationId IS NULL
            AND r.LocalCountryId IS NOT NULL
    ),
    NewLocationGroups AS (
        SELECT
            newGroups.Iata,
            newGroups.SourceOrder,
            @StartId + ROW_NUMBER() OVER (ORDER BY newGroups.Iata, newGroups.SourceOrder) AS NewLocationId
        FROM (
            SELECT DISTINCT
                Iata,
                SourceOrder
            FROM InsertCandidates
            WHERE ExistingAnyLanguageLocationId IS NULL
        ) newGroups
    ),
    Insertable AS (
        SELECT
            candidates.*,
            COALESCE(candidates.ExistingAnyLanguageLocationId, groups.NewLocationId) AS LocationIdToInsert
        FROM InsertCandidates candidates
        LEFT JOIN NewLocationGroups groups
            ON groups.Iata = candidates.Iata
            AND groups.SourceOrder = candidates.SourceOrder
    )
    INSERT INTO LOCATION (
        ID,
        ACTIVE,
        LANGID,
        COUNTRYID,
        CITYID,
        LOCATIONNAME,
        IATA,
        AIRPORT,
        ISPICKUP,
        MAILADDRESS,
        ADDRESS,
        PHONENUMBER,
        COORDINATELATITUDE,
        COORDINATELONGITUDE,
        ISPOPULAR,
        IMAGEPATH,
        LOCATIONORDER,
        ISDEFAULT,
        ISCITYDEFAULTLOCATION,
        GOOGLEPLACEID,
        STATEID,
        ISDELETED
    )
    SELECT
        LocationIdToInsert,
        1,
        LangId,
        LocalCountryId,
        CityId,
        LEFT(LocationName, 200),
        Iata,
        1,
        1,
        NULL,
        NULL,
        NULL,
        NULL,
        NULL,
        0,
        NULL,
        NULL,
        0,
        NULL,
        NULL,
        NULL,
        0
    FROM Insertable
    ORDER BY Iata, SourceOrder, LangId;

    SELECT @@ROWCOUNT AS InsertedCount;

    COMMIT TRANSACTION;
END
ELSE
BEGIN
    SELECT N'PreviewOnly' AS Status, COUNT(*) AS InsertCandidateCount
    FROM @Resolved
    WHERE DuplicateRank = 1
        AND ExistingLocationId IS NULL
        AND LocalCountryId IS NOT NULL;
END
"@

Set-Content -LiteralPath $OutputPath -Value $sql -Encoding UTF8

[pscustomobject]@{
    SourceCount = $airports.Count
    ValidCount = $validAirports.Count
    OutputPath = (Resolve-Path -LiteralPath $OutputPath).Path
}
