using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Response.Avec3ResponseBase;

namespace KolayCAR.Broker.API.Providers.Avec3
{
    public class AuthProvider
    {
        public HttpManager _httpManager;
        public AuthProvider(string apiBaseUrl)
        {
            _httpManager = new HttpManager(apiBaseUrl);
        }
        public async Task<HttpResult<AuthResponseBase>> GetTokenAsync(string apiKey, string apiPassword)
        {
            var authResult = await _httpManager.PostAsync<AuthResponseBase>(
                            requestPath: @"user/auth",
                            headers: AuthRequestHeader(),
                            content: GetUrlEncodedContent(apiKey, apiPassword));

            if (!authResult.Success)
                Serilog.Log
                .ForContext("{@UserAuthenticate}", new Avec3RequestBase.UserAuthenticate
                {
                    username = apiKey,
                    password = apiPassword
                })
                .Fatal("{@BrokerAuth}", authResult);

            return authResult;
        }
        public Dictionary<string, object> AuthRequestHeader() =>
           new Dictionary<string, object>()
           {
                { "x-os-version", "" },
                { "x-os-name", "" },
                { "x-app-version", "" },
                { "accept-language", "" },
                { "x-device-id", "" },
                { "x-geo-point", "" },
                { "x-device-model", "" },
                { "x-market-store", "" },
                { "Authorization", "" }
           };
        public FormUrlEncodedContent GetUrlEncodedContent(string apiKey, string apiPassword)
        {
            return new FormUrlEncodedContent(new[] {
                              new KeyValuePair<string,string>("grant_type","password"),
                              new KeyValuePair<string,string>("username",apiKey),
                              new KeyValuePair<string,string>("password",apiPassword)
                            });
        }
        public Dictionary<string, object> CreateAuthHeader(string token) =>
            new Dictionary<string, object>()
            {
                { "Authorization", $"Bearer {token}"}
            };


    }
}
