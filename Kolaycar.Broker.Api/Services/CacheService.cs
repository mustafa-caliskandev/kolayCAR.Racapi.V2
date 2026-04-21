using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Infrastructure.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace KolayCAR.Broker.API.Services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;

        private const string _cacheKeys = "_cacheKeys";

        public CacheService(IMemoryCache cache, IConfiguration configuration)
        {
            _cache = cache;
            _configuration = configuration;
        }

        public async Task SetAsync<T>(string key, T data, TimeSpan? absoluteExpiration = null)
        {
            if (data == null) return;

            var defaultCacheExpireTime = _configuration["AppSettings:DefaultCacheExpire"].ToIntNullSafe();
            var defaultHours = defaultCacheExpireTime == 0 ? 12 : defaultCacheExpireTime;

            _cache.Set(key, data, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration ?? TimeSpan.FromHours(defaultHours)
            });

            await AddKeyToCacheListAsync(key);
        }

        public Task<T> GetAsync<T>(string key)
        {
            return Task.FromResult(_cache.TryGetValue(key, out T value) ? value : default);
        }

        public Task RemoveAsync(string key)
        {
            _cache.Remove(key);
            return Task.CompletedTask;
        }

        public Task RemoveByPrefixAsync(string prefixKey)
        {
            if (_cache.TryGetValue(_cacheKeys, out List<string> cacheKeyList))
            {
                var keysToRemove = cacheKeyList.Where(k => k.StartsWith(prefixKey)).ToList();
                foreach (var key in keysToRemove)
                {
                    _cache.Remove(key);
                    cacheKeyList.Remove(key);
                }
            }
            return Task.CompletedTask;
        }

        public Task ClearAsync(CacheTypes cacheType)
        {
            if (_cache.TryGetValue(_cacheKeys, out List<string> cacheKeyList))
            {
                foreach (var key in cacheKeyList)
                {
                    if (key.Contains(cacheType.ToString()) || cacheType == CacheTypes.Full)
                        _cache.Remove(key);
                }
            }

            return Task.CompletedTask;
        }

        public async Task<T> GetOrCreateAsync<T>(
            string key,
            Func<Task<T>> createItem,
            TimeSpan? expiration = null)
        {
            if (_cache.TryGetValue(key, out T existing))
                return existing;

            var value = await createItem();
            await SetAsync(key, value, expiration);
            return value;
        }

        public Task ValidateCacheAsync()
        {
            return Task.CompletedTask;
        }

        public Task InvalidateCacheAsync(CacheTypes cacheType)
        {
            return Task.CompletedTask;
        }

        public Task ClearAllCacheAsync()
        {
            if (_cache.TryGetValue(_cacheKeys, out List<string> cacheKeyList))
            {
                foreach (var key in cacheKeyList)
                {
                    _cache.Remove(key);
                }
                _cache.Remove(_cacheKeys);
            }
            return Task.CompletedTask;
        }

        private async Task AddKeyToCacheListAsync(string cacheKey)
        {
            var cacheKeyList = await GetAsync<List<string>>(_cacheKeys) ?? new List<string>();

            if (!cacheKeyList.Contains(cacheKey))
            {
                cacheKeyList.Add(cacheKey);
                _cache.Set(_cacheKeys, cacheKeyList);
            }
        }

        public async Task<string> GetAllCacheAsJsonAsync()
        {
            var snapshot = new Dictionary<string, object>();

            if (_cache.TryGetValue(_cacheKeys, out List<string> cacheKeyList))
            {
                foreach (var key in cacheKeyList)
                {
                    if (_cache.TryGetValue(key, out object value))
                        snapshot[key] = value;
                }
            }

            var json = JsonConvert.SerializeObject(snapshot, Formatting.Indented);

            return json.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace("\"", "").Replace("\\r", "").Replace("\\n", "");
        }
    }
}