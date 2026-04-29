namespace KolayCAR.Broker.API.Helpers;

public static class CacheSettings
{
    private static bool _isLocked = false;
    public static string AllKey { get; private set; } = "FUL9AX";
    public static string AgencyKey { get; private set; } = "A7K9Q2";
    public static string VendorKey { get; private set; } = "V4M8ZL";
    public static string VehicleVendorKey { get; private set; } = "VHC9AL";
    public static string LocationKey { get; private set; } = "LBZ3AY";
    public static string LocationVendor { get; private set; } = "LVT2ML";
    public static string ConfigurationKey { get; private set; } = "SKPL4A";
    public static string MarkupKey { get; private set; } = "MKP5X1";
    public static string SpecialRequestKey { get; private set; } = "SPRQST4";
    public static string SupplierLogRules { get; private set; } = "SLR8QX";
    public static string ExtraVendor { get; private set; } = "EXV4ML";
    public static string BrokerName { get; private set; }
    public static bool UseCache { get; private set; }

    public static void Initialize(bool useCache, string brokerName)
    {
        if (_isLocked)
            return;

        UseCache = useCache;
        BrokerName = brokerName;

        _isLocked = true;
    }

    public static bool IsAllKey(string cacheKey)
    {
        return string.Equals(cacheKey?.Trim(), AllKey, System.StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsValidCacheKey(string cacheKey)
    {
        if (string.IsNullOrWhiteSpace(cacheKey))
            return false;

        var normalizedCacheKey = cacheKey.Trim();

        return string.Equals(normalizedCacheKey, AllKey, System.StringComparison.OrdinalIgnoreCase)
               || string.Equals(normalizedCacheKey, AgencyKey, System.StringComparison.OrdinalIgnoreCase)
               || string.Equals(normalizedCacheKey, VendorKey, System.StringComparison.OrdinalIgnoreCase)
               || string.Equals(normalizedCacheKey, VehicleVendorKey, System.StringComparison.OrdinalIgnoreCase)
               || string.Equals(normalizedCacheKey, LocationKey, System.StringComparison.OrdinalIgnoreCase)
               || string.Equals(normalizedCacheKey, LocationVendor, System.StringComparison.OrdinalIgnoreCase)
               || string.Equals(normalizedCacheKey, ConfigurationKey, System.StringComparison.OrdinalIgnoreCase)
               || string.Equals(normalizedCacheKey, MarkupKey, System.StringComparison.OrdinalIgnoreCase)
               || string.Equals(normalizedCacheKey, SpecialRequestKey, System.StringComparison.OrdinalIgnoreCase)
               || string.Equals(normalizedCacheKey, SupplierLogRules, System.StringComparison.OrdinalIgnoreCase)
               || string.Equals(normalizedCacheKey, ExtraVendor, System.StringComparison.OrdinalIgnoreCase);
    }
}
