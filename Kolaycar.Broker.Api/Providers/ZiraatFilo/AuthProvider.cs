using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Providers.ZiraatFilo
{
    public class AuthProvider
    {
        private readonly HttpManager _httpManager;

        public AuthProvider(string apiBaseUrl)
        {
            _httpManager = new HttpManager(apiBaseUrl);
        }

        public async Task<IDictionary<string, object>> GetHeaders(Vendor vendor)
        {
            //var token = await _httpManager.PostAsyncWithModel<AuthRequest, AuthResponse>(
            //    requestPath: "/api/auth/login",
            //    entity: new AuthRequest
            //    {
            //        service_name = vendor.ApiClientId,
            //        username = vendor.ApiKey,
            //        password = vendor.ApiPassword
            //    });

            //if (token == null)
            //    return null;

            return new Dictionary<string, object>
            {
                { "Authorization", $"Token {vendor.ApiKey}" },
                { "company-id", vendor.ApiPassword },
            };
        }
    }
}

