using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Response.Circular2ResponseBase;

namespace KolayCAR.Broker.API.Providers.Circular2
{
    public class AuthProvider
    {
        public readonly HttpManager _httpManager;
        public AuthProvider(string apiBaseUrl)
        {
            _httpManager = new HttpManager(apiBaseUrl);
        }

        public async Task<AuthResponse> GetTokenAsync(string apiKey)
        {
            var result = await _httpManager.GetAsync2<AuthResponse>(
                requestPath: $"/auth/get_token?apikey={apiKey}"
                );
            if (result == null) {
                return null;
            }
            if (result?.Data?.success != false)
            {
                return result.Data;
            }
            else
            {
                return null;
            }
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
