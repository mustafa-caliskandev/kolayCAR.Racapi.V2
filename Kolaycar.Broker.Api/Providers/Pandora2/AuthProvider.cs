using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Pandora2
{
    public class AuthProvider
    {
        private RestManager RestManager { get; set; }

        public AuthProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(NormalizeBaseUrl(apiBaseUrl));
        }

        public async Task<Pandora2ResponseBase.Token> GetAccessToken(Vendor vendor)
        {
            return await RestManager.PostAsyncRestClient<Pandora2ResponseBase.Token>(
                requestPath: "access_token",
                parameterType: ParameterType.RequestBody,
                headers: CreateJsonHeader(),
                entity: CreateJsonBody(new AccessTokenRequest
                {
                    grant_type = "password",
                    client_id = vendor.ApiClientId.ToIntNullSafe(),
                    client_secret = vendor.SecretKey,
                    username = vendor.ApiKey,
                    password = vendor.ApiPassword
                }));
        }

        public Dictionary<string, object> CreateJsonHeader(string languageCode = null) =>
            new Dictionary<string, object>
            {
                { "Content-Type", "application/json" },
                { "lang", GetLanguageCode(languageCode) }
            };

        public Dictionary<string, object> CreateAuthHeader(string accessToken, string languageCode = null) =>
            new Dictionary<string, object>
            {
                { "Auth", $"Bearer {accessToken}" },
                { "lang", GetLanguageCode(languageCode) }
            };

        public Dictionary<string, object> CreateAuthHeaderWithContentType(string accessToken, string languageCode = null) =>
            new Dictionary<string, object>
            {
                { "Auth", $"Bearer {accessToken}" },
                { "Content-Type", "application/json" },
                { "lang", GetLanguageCode(languageCode) }
            };

        public Dictionary<string, object> CreateJsonBody(object entity) =>
            new Dictionary<string, object>
            {
                { "application/json", JsonConvert.SerializeObject(entity) }
            };

        private static string GetLanguageCode(string languageCode)
        {
            var code = languageCode.ToStringNullSafe().Trim().ToLowerInvariant();
            return code == "en" || code == "de" || code == "tr" ? code : "tr";
        }

        internal static string NormalizeBaseUrl(string apiBaseUrl)
        {
            var normalizedApiBaseUrl = apiBaseUrl.ToStringNullSafe().Trim();
            return normalizedApiBaseUrl.EndsWith("/") ? normalizedApiBaseUrl : $"{normalizedApiBaseUrl}/";
        }
    }

    public class AccessTokenRequest
    {
        public string grant_type { get; set; }
        public int client_id { get; set; }
        public string client_secret { get; set; }
        public string username { get; set; }
        public string password { get; set; }
    }
}
