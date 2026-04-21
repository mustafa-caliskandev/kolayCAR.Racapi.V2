using KolayCAR.Broker.Infrastructure.Managers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Requests.Yolcu360RequestBase;

namespace KolayCAR.Broker.API.Providers.Yolcu360
{
    public class AuthProvider
    {
        RestManager RestManager { get; set; }
        private readonly IMemoryCache _memoryCache;
        const string Auth_Cache_Key = "YOLCU360_AUTH";
        public AuthProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
        }

        public AuthProvider(string apiBaseUrl, IMemoryCache memoryCache)
        {
            RestManager = new RestManager(apiBaseUrl);
            _memoryCache = memoryCache;
        }

        public async Task<string> Login(string email, string password, bool clearCache = false)
        {
            #region Yolcu session cache dönderme veya tanımlamalar
            string cookie = "";
            string expireDateString = "";
            string cacheKey = string.Format(Auth_Cache_Key, 1);
            if (!clearCache && _memoryCache != null && _memoryCache.TryGetValue(cacheKey, out string cacheToken))
            {
                return cacheToken;
            }
            #endregion

            var authResult = await RestManager.PostAsyncWithCookie<EmailLoginParameters, IEnumerable<string>>(
                requestPath: "auth/login/",
                headers: CreateHeaderWithContentType(),
                entity: CreateBody(email, password)
                //,isHeaderResponse: true
                );

            if (authResult != null && authResult.Count() > 0)
            {
                foreach (var item in authResult)
                {
                    if (item.Contains("sessionid"))
                    {
                        expireDateString = item.Split(';')[1].Split('=')[1];
                        cookie += item.Split(';')[0] + ";";
                    }
                }
            }

            #region Yolcu servisinden dönen sessionId son kullanma tarihine göre cache işlemi
            DateTime expireDate;
            var isDateConverted = DateTime.TryParseExact(expireDateString, "ddd, dd-MMM-yyyy HH':'mm':'ss 'GMT'",
                                                CultureInfo.InvariantCulture,
                                                DateTimeStyles.None, out expireDate);
            if (isDateConverted)
            {
                var expireDay = Math.Floor((expireDate - DateTime.Now).TotalDays);
                isDateConverted = expireDay > 0;
            }

            if (_memoryCache != null && !string.IsNullOrEmpty(cookie))
            {
                _memoryCache.Set(cacheKey, cookie, new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = isDateConverted ? DateTime.Now.AddDays(Math.Floor((expireDate - DateTime.Now).TotalDays)) : DateTime.Now.AddDays(1),
                    Priority = CacheItemPriority.Normal
                });
            }
            #endregion

            return cookie;
        }

        public async Task<object> LogOut()
        {
            var logoutResult = await RestManager.PostAsync<object, object>(
                requestPath: "auth/logout"
                );

            return logoutResult;
        }

        public Dictionary<string, object> CreateHeaderWithContentType()
        {
            return new Dictionary<string, object>()
            {
                { "Content-Type", "application/json"},
                { "Accept", "application/json"}
            };
        }

        public EmailLoginParameters CreateBody(string email, string password)
        {
            return new EmailLoginParameters
            {
                email = email,
                password = password
            };
        }

        public Dictionary<string, object> CreateHeaderWithToken(string token)
        {
            return new Dictionary<string, object>
            {
                { "api_key", token }
            };
        }
        public Dictionary<string, object> CreateHeaderWithCookie(string cookie)
        {
            return new Dictionary<string, object>
            {
                { "Cookie", cookie },
                { "Accept", "application/json" }
            };
        }
        public Dictionary<string, object> CreateReservationHeaderWithCookie(string cookie)
        {
            return new Dictionary<string, object>
            {
                { "Cookie", cookie },
                { "Accept", "application/json" },
                { "Content-Type", "application/json"}
            };
        }
        public Dictionary<string, object> CreateReservationHeaderWithSessionId(string cookie)
        {
            string sessionId = "";
            foreach (var item in cookie.Split(';'))
            {
                if (item.Contains("session"))
                    sessionId = item;
            }

            return new Dictionary<string, object>
            {
                { "Cookie", sessionId },
                { "Accept", "application/json" },
                { "Content-Type", "application/json"}
            };
        }

    }
}
