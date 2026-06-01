namespace Kolaycar.Broker.Api.Helpers.YesOto
{
    public static class YesOtoConstants
    {
        public const string BaseApiUrl = "https://api.enterprise.com.tr";
        public const string IdentityBaseUrl = "https://identity.enterprise.com.tr";
        public const string BrandId = "812633FA-FA4B-0029-99B4-39F40BDCB938";
        public const string SalesChannelId = "68010AF6-47E7-5EBB-5A12-3A07DA8486DF";

        public static string NormalizeApiBaseUrl(string apiBaseUrl)
        {
            if (string.IsNullOrWhiteSpace(apiBaseUrl))
                return BaseApiUrl;

            return apiBaseUrl.Replace("identity.enterprise.com.tr", "api.enterprise.com.tr");
        }

        public static string NormalizeIdentityBaseUrl(string apiBaseUrl)
        {
            if (string.IsNullOrWhiteSpace(apiBaseUrl))
                return IdentityBaseUrl;

            return apiBaseUrl.Replace("api.enterprise.com.tr", "identity.enterprise.com.tr");
        }
    }
}
