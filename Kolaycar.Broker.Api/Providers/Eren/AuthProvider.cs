using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Responses.Eren;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kolaycar.Broker.Api.Providers.Eren
{
    public class AuthProvider
    {
        private readonly HttpManager _httpManager;

        public AuthProvider(string apiBaseUrl)
        {
            _httpManager = new HttpManager(apiBaseUrl);
        }

        public async Task<Dictionary<string, object>> GetTokenAsync(Vendor vendor)
        {
            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("username", vendor.ApiKey),
                new KeyValuePair<string, string>("password", vendor.ApiPassword)
            };

            var headers = new Dictionary<string, object>
            {
                { "Content-Type", "application/x-www-form-urlencoded" }
            };

            var response = await _httpManager.PostAsyncWithUrlEncoded<object, ErenTokenResponse>(
                "/v1/oauth/token",
                entity: formData,
                headers: headers
            );

            if (response != null && !string.IsNullOrEmpty(response.AccessToken))
            {
                return new Dictionary<string, object>
                {
                    { "Content-Type", "application/json" },
                    { "Authorization", $"Bearer {response.AccessToken}" }
                };
            }

            return null;
        }

    }
}
