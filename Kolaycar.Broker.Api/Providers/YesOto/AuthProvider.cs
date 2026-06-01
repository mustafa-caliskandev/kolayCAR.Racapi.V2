using Kolaycar.Broker.Api.Helpers.YesOto;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Responses.YesOto;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kolaycar.Broker.Api.Providers.YesOto
{
    public class AuthProvider
    {
        private readonly HttpManager _httpManager;

        public AuthProvider(string apiBaseUrl)
        {
            _httpManager = new HttpManager(YesOtoConstants.NormalizeIdentityBaseUrl(apiBaseUrl));
        }

        public async Task<string> GetTokenAsync(Vendor vendor)
        {
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", vendor.ApiKey),
                new KeyValuePair<string, string>("client_secret", vendor.ApiPassword)
            };

            var parameters = new Dictionary<string, object>();
            var headers = new Dictionary<string, object>();

            var response = await _httpManager.PostAsyncWithUrlEncoded<object, YesOtoTokenResponse>(
                "/connect/token",
                request,
                parameters,
                headers
            );

            if (response != null && !string.IsNullOrEmpty(response.AccessToken))
            {
                return response.AccessToken;
            }

            return null;
        }
    }
}
