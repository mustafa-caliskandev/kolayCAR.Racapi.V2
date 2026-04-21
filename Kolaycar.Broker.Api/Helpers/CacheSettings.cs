namespace KolayCAR.Broker.API.Helpers;

public static class CacheSettings
{
    private static bool _isLocked = false;

    public static string AgencyKey { get; private set; } = "A7K9Q2";
    public static string VendorKey { get; private set; } = "V4M8ZL";
    public static string VehicleVendorKey { get; private set; } = "VHC9AL";
    public static string LocationKey { get; private set; } = "LBZ3AY";
    public static string ConfigurationKey { get; private set; } = "SKPL4A";
    public static string MarkupKey { get; private set; } = "MKP5X1";
    public static string SpecialRequestKey { get; private set; } = "SPRQST4";
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
}