using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Requests.TurmobilRequestBase;
using static KolayCAR.Broker.Domain.Models.Response.TurmobilResponseBase;

namespace KolayCAR.Broker.API.Providers.Turmobil
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
        public async Task<AuthLoginResponse> GetToken(string user, string password)
        {
            var authResult = await RestManager.PostAsyncWithHeader<AuthLoginRequest, AuthLoginResponse>(
                            requestPath: "authentication/login",
                            entity: new AuthLoginRequest
                            {
                                user = user,
                                password = password
                            },
                            headers: CreateHeaderWithContentType());

            return new AuthLoginResponse
            {
                Token = authResult
            };
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
                { "Authorization", $"{token}"},
                { "Content-Type", "application/json"},
                { "Accept", "application/json"}
              };

        public Dictionary<string, object> CreateHeaderWithUserInfo(string username, string password)
        {
            return new Dictionary<string, object>()
            {
                { "username", username },
                { "password", password }
            };
        }
    }
}
