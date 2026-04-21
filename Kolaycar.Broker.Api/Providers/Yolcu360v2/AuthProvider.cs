using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response.Yolcu360v2;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Yolcu360v2
{
    public class AuthProvider
    {
        private HttpManager _httpManager;
        private ICacheService _cacheService;
        public AuthProvider(string apiBaseUrl, ICacheService cacheService)
        {
            _httpManager = new HttpManager(apiBaseUrl);
            _cacheService = cacheService;
        }

        public async Task<IDictionary<string, object>> GetToken(Vendor vendor)
        {
            var auth = await _cacheService.GetOrCreateAsync($"Yolcu360v2Token", () => _httpManager.PostAsyncWithModel<Yolcu360v2RequestBase.Yolcu360v2AuthRequest, Yolcu360v2AuthResponse>(
                     requestPath: "/api/v1/auth/login",
                     entity: new Yolcu360v2RequestBase.Yolcu360v2AuthRequest
                     {
                         key = vendor.ApiKey,
                         secret = vendor.ApiPassword
                     }
                 ), TimeSpan.FromMinutes(30));

            return new Dictionary<string, object>
                {
                    { "Authorization", $"Bearer {auth.accessToken}" }
                };
        }
    }
}
