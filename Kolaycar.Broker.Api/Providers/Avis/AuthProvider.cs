using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Response.AvisResponseBase;

namespace KolayCAR.Broker.API.Providers.Avis
{
    public class AuthProvider
    {
        RestManager _restManager;
        public AuthProvider(string apiBaseUrl)
        {
            _restManager = new RestManager(apiBaseUrl);
        }
        public async Task<AvisAuthResponse> GetTokenAsync(Vendor vendor)
        {
            var token = await _restManager.PostFormUrlEncoded<AvisAuthResponse>
                        (
                         requestPath: "token",
                         postData: GetParameters(vendor),
                         headers: CreateHeaderWithContentType()
                        );
            if (token == null)
                return null;

            return token;
        }

        private IEnumerable<KeyValuePair<string, string>> GetParameters(Vendor vendor) => 
            new Dictionary<string, string>()
            {
                  { "username", $"{vendor.ApiKey}"},
                  { "password", $"{vendor.ApiPassword}" },
                  { "grant_type", "password" }
            };

        public IDictionary<string, object> CreateAuthHeaderWithContentType(AvisAuthResponse avisAuthResponse) =>
            new Dictionary<string, object>()
            {
                  { "Authorization", $"Bearer {avisAuthResponse.access_token}"},
            };
        private IDictionary<string, object> CreateHeaderWithContentType() =>
            new Dictionary<string, object>()
            {
                  { "Content-Type", "application/x-www-form-urlencoded"},
                  { "Accept", "application/json"}
            };
    }
}
