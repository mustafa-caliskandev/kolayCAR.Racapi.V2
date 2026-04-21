using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Requests.CizgiRequestBase;
using static KolayCAR.Broker.Domain.Models.Response.CizgiResponseBase;

namespace KolayCAR.Broker.API.Providers.Cizgi
{
    public class AuthProvider
    {
        RestManager RestManager { get; set; }
        HttpManager HttpManager { get; set; }
        public AuthProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
            HttpManager = new HttpManager(apiBaseUrl);
        }
        public async Task<AuthLoginResponse> GetToken(string brokerId, string secretKey)
        {
            var authResult = await RestManager.PostAsync<AuthLoginRequest, AuthLoginResponse>(
                            requestPath: "auth/login",
                            entity: new AuthLoginRequest
                            {
                                brokerid = brokerId,
                                secret = secretKey
                            },
                            headers: CreateHeaderWithContentType());

            return authResult;
        }

        public Dictionary<string, object> CreateHeaderWithContentType() =>
               new Dictionary<string, object>()
               {
                { "Content-Type", "application/json"},
                { "Accept", "application/json"}
               };

        public Dictionary<string, object> CreateAuthHeaderWithContentType(string token) =>
              new Dictionary<string, object>()
              {
                { "Authorization", $"Bearer {token}"},
                { "Content-Type", "application/json"},
                { "Accept", "application/json"}
              };
    }
}
