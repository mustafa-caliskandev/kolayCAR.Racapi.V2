using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Sixt2
{
    public class AuthProvider
    {
        HttpManager _httpManager;
        ICacheService _cacheService;
        public AuthProvider(string apiBaseUrl, ICacheService cacheService)
        {
            _httpManager = new HttpManager(apiBaseUrl);
            _cacheService = cacheService;
        }

        public async Task<Dictionary<string, object>> GetBearerToken(Domain.Models.Vendor vendor)
        {
            const string cacheKey = "SixtToken";

            var cachedToken = await _cacheService.GetAsync<HttpResult<SixtResponseBase<LoginResult>>>(cacheKey);

            if (cachedToken?.Data?.result != null)
                return BuildAuthHeaders(cachedToken.Data.result.accessToken, cachedToken.Data.result.expires_in, cachedToken.Cookie);

            var authResult = await _httpManager.PostAsync2<SixtRequestBase.SixtLoginRequest, SixtResponseBase<LoginResult>>(
                "/api/v1/login",
                entity: new SixtRequestBase.SixtLoginRequest
                {
                    email = vendor.ApiKey,
                    password = vendor.ApiPassword
                },
                getCookie: true
            );

            if (authResult?.Data?.result != null)
            {
                await _cacheService.SetAsync(cacheKey, authResult, TimeSpan.FromSeconds(authResult.Data.result.expires_in));
                return BuildAuthHeaders(authResult.Data.result.accessToken, authResult.Data.result.expires_in, authResult.Cookie);
            }

            return null;
        }

        private Dictionary<string, object> BuildAuthHeaders(string accessToken, int expiresIn, string cookie)
        {
            var headers = new Dictionary<string, object>
                {
                    { "Authorization", $"Bearer {accessToken}" },
                    { "Accept", "application/json" }
                };
            if (!string.IsNullOrEmpty(cookie))
            {
                headers.Add("Cookie", cookie);
            }
            return headers;
        }
    }
}
