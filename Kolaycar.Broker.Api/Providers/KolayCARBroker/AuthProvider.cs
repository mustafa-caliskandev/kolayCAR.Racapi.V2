using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.KolayCARBroker
{
    public class AuthProvider
    {
        HttpManager HttpManager { get; set; }
        public AuthProvider(string apiBaseUrl)
        {
            HttpManager = new HttpManager(apiBaseUrl);
        }

        public async Task<HttpResult<User>> GetJWT(string apiKey, string apiPassword, string secretKey)
        {
            var authResult = await HttpManager.PostAsync<UserAuthenticate, User>(
                            requestPath: "users/authenticate",
                            entity: new UserAuthenticate
                            {
                                Username = apiKey,
                                Password = apiPassword,
                                SecretKey = secretKey
                            });

            if ((bool)!authResult?.Success)
                //Serilog.Log
                //.ForContext("{@UserAuthenticate}", new UserAuthenticate
                //{
                //    Username = apiKey,
                //    Password = apiPassword
                //})
                //.Fatal("{@BrokerAuth}", authResult);
                Serilog.Log.Error("{@BrokerAuth}", $"apiKey : {apiKey} - apiPassword : {apiPassword}");
            return authResult;
        }

        public Dictionary<string, object> CreateAuthHeader(string jwt) =>
            new Dictionary<string, object>()
            {
                { "Authorization", $"Bearer {jwt}"}
            };
    }
}
