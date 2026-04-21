using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Response.PandoraResponseBase;

namespace KolayCAR.Broker.API.Providers.Eganis
{
    public class AuthProvider
    {

        RestManager RestManager { get; set; }

        public AuthProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
        }

        public async Task <IDictionary<string, object>> getToken(Domain.Models.Vendor vendor)
        {
            var loginRequest = new EganisRequestBase.AuthLoginRequest
            {
                key = vendor.SecretKey,
                userName = vendor.ApiKey,
                password = vendor.ApiPassword,
            };

            var result = await RestManager.PostAsync<EganisRequestBase.AuthLoginRequest,EganisResponseBase.AuthLoginResponse>
                (
                    requestPath: "/Api/Login",
                    entity: loginRequest
                );

            if(result?.data != null && result.isSucceed == false)
            {
                return null;
            }

            return new Dictionary<string, object>
                {
                    { "Authorization", $"Bearer {result.data}" }
                };

        }
    }
}

