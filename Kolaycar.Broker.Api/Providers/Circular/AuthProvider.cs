using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Requests.CircularRequestBase;
using static KolayCAR.Broker.Domain.Models.Response.CircularResponseBase;

namespace KolayCAR.Broker.API.Providers.Circular
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
                            requestPath: "/auth/get_token?apikey=" + brokerId,
                            entity: new AuthLoginRequest
                            {
                                brokerid = brokerId,
                                secret = secretKey
                            },
                            headers: CreateHeaderWithContentType());
            //var authResult2 = await HttpManager.GetAsync2<AuthLoginResponse>(
            //    requestPath: "/auth/get_token?apikey=" + brokerId);
            //var authResult = new AuthLoginResponse { token = authResult2.Data.token, message = authResult2.Data.message , 
            // success = authResult2.Data.success, id = authResult2.Data.id};
            return authResult;
        }

        public Dictionary<string, object> CreateHeaderWithContentType() =>
               new Dictionary<string, object>()
               {
                { "Content-Type", "application/x-www-form-urlencoded"},
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
