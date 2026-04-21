using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Response.BetoResponseBase;

namespace KolayCAR.Broker.API.Providers.Beto
{
    public class AuthProvider
    {
        RestManager RestManager { get; set; }
        public AuthProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
        }
        public async Task<AuthResponseBase> GetToken(string username, string password)
        {
            var authResult = await RestManager.PostFormUrlEncoded<AuthResponseBase>(
                            requestPath: "token",
                            postData: new[] {
                              new KeyValuePair<string,string>("grant_type","password"),
                              new KeyValuePair<string,string>("username",username),
                              new KeyValuePair<string,string>("password",password)
                            });

            return authResult;
        }

        public Dictionary<string, object> CreateHeaderWithContentType() =>
               new Dictionary<string, object>()
               {
                { "Content-Type", "application/x-www-form-urlencoded"}
               };

        public Dictionary<string, object> CreateAuthHeaderWithContentType(string token) =>
              new Dictionary<string, object>()
              {
                { "Authorization", $"Bearer {token}"},
                { "Content-Type", "application/x-www-form-urlencoded"}
              };
        public Dictionary<string, object> CreateAuthHeaderWithContentTypeJson(string token) =>
              new Dictionary<string, object>()
              {
                { "Authorization", $"Bearer {token}"},
                { "Content-Type", "application/json"}
              };
    }
}
