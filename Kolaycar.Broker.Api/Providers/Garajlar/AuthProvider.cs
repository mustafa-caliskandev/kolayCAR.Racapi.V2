using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Requests.GarajlarRequestBase;
using static KolayCAR.Broker.Domain.Models.Response.GarajlarResponseBase;

namespace KolayCAR.Broker.API.Providers.Garajlar
{
    public class AuthProvider
    {
        private readonly HttpManager _httpManager;
        public AuthProvider(string apiBaseUrl)
        {
            _httpManager = new HttpManager(apiBaseUrl);
        }

        public async Task<IDictionary<string, object>> GetTokenHeader(Domain.Models.Vendor vendor)
        {
            var token = await _httpManager.PostAsyncWithModel<AuthRequest, ResponseBase<string>>("/api/obilet/login", entity: new() { email = vendor.ApiKey, password = vendor.ApiPassword });
            if (!string.IsNullOrEmpty(token?.data))
                return GetParameters(token.data);
            return null;
        }

        public IDictionary<string, object> GetParameters(string token) =>
        new Dictionary<string, object>()
        {
                { "Authorization", $"Bearer {token}"}
        };
    }
}
