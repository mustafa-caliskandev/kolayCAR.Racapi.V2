using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Renticar.Request;
using KolayCAR.Broker.Domain.Models.Renticar.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Renticar
{
    public class AuthProvider
    {
        RestManager RestManager { get; set; }
        private readonly IMemoryCache _memoryCache;
        const string Auth_Cache_Key = "RENTICAR_AUTH";
        public AuthProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
        }
        public AuthProvider(string apiBaseUrl, IMemoryCache memoryCache)
        {
            RestManager = new RestManager(apiBaseUrl);
            _memoryCache = memoryCache;
        }

        public async Task<LoginResponseBase> GetToken(Vendor vendor, bool clearCahce = false)
        {
            LoginResponseBase result = null;
            if (_memoryCache != null)
            {
                if (!clearCahce && _memoryCache.TryGetValue(Auth_Cache_Key, out LoginResponseBase cacheObj))
                {
                    return cacheObj;
                }

                result = await RestManager.PostAsync<LoginRequestBase, LoginResponseBase>(
                requestPath: "login",
                entity: CreateLoginRequestBody(vendor)
                );

                if (result != null && result.status == "success")
                {
                    _memoryCache.Set(Auth_Cache_Key, result, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpiration = DateTime.Now.AddHours(12),
                        Priority = CacheItemPriority.Normal
                    });

                    return result;
                }
            }

            result = await RestManager.PostAsync<LoginRequestBase, LoginResponseBase>(
                requestPath: "login",
                entity: CreateLoginRequestBody(vendor)
                );

            return result;
        }

        private static LoginRequestBase CreateLoginRequestBody(Vendor vendor) => new LoginRequestBase
        {
            email = vendor.ApiKey,
            password = vendor.ApiPassword
        };

        public static Dictionary<string, object> CreateHeader(string token) => new Dictionary<string, object> {
            { "Authorization", $"Bearer {token}" }
        };
    }
}
