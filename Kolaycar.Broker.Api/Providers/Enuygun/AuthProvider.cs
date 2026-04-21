using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Enuygun
{
    public class AuthProvider
    {

        RestManager RestManager { get; set; }

        public AuthProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
        }

        public async Task<EnuygunResponse.Login.Root> GetToken(string username, string password)
        {
            var loginRequest = new EnuygunRequest.Login
            {
                username = username,
                password = password

            };


            var result = await RestManager.PostAsync<EnuygunRequest.Login, EnuygunResponse.Login.Root>(
                            requestPath: "/api/v1/login",
                            entity: loginRequest,
                            headers: CreateHeaderWithContentType()
                            );

            if (result == null || string.IsNullOrEmpty(result.Data.Token))
            {

                throw new System.Exception("Kimlik doğrulama başarısız oldu, Token alınamadı.");

            }

            return result;
        }

        private Dictionary<string, object> CreateHeaderWithContentType()
        {

            var headers = new Dictionary<string, object>
            {

                { "Content-Type", "application/json" },
                { "Accept", "*/*" },
                { "Connection", "keep-alive" }
            };

            return headers;
        }

        public Dictionary<string, object> CreateHeaderWithToken(string token)
        {

            var headers = new Dictionary<string, object>
            {

                { "Content-Type", "application/json" },
                { "Accept", "*/*" },
                { "Connection", "keep-alive" },
                { "Authorization", $"Bearer {token}" }
            };

            return headers;
        }
    }
}
