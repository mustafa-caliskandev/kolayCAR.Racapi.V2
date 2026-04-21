using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Infrastructure.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public class MultiInstanceCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private static readonly Dictionary<CacheTypes, string> _cacheFiles = new()
        {
            { CacheTypes.Full, "Docs\\CacheFile\\CacheFile.txt" },
            { CacheTypes.Agency, "Docs\\CacheFile\\AgencyCacheFile.txt" },
            { CacheTypes.Location, "Docs\\CacheFile\\LocationCacheFile.txt" },
            { CacheTypes.Vendor, "Docs\\CacheFile\\VendorCacheFile.txt" },
            { CacheTypes.Markup, "Docs\\CacheFile\\MarkupCacheFile.txt" }
        };
        private const string _cacheKeys = "_cacheKeys";

        public MultiInstanceCacheService(IMemoryCache cache, IConfiguration configuration)
        {
            _cache = cache;
            _configuration = configuration;
            EnsureCacheFilesExist();
        }

        public async Task SetAsync<T>(string key, T data, TimeSpan? absoluteExpiration = null)
        {
            if (data == null) return;
            var defaultCacheExpireTime = _configuration["AppSettings:DefaultCacheExpire"].ToIntNullSafe();
            var defaultCacheExpire = defaultCacheExpireTime == 0 ? 12 : defaultCacheExpireTime;
            _cache.Set(key, data, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = absoluteExpiration ?? TimeSpan.FromHours(defaultCacheExpire) });
            await AddKeyToCacheListAsync(key);
        }

        public Task<T> GetAsync<T>(string key) => Task.FromResult(_cache.TryGetValue(key, out T data) ? data : default);
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

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> createItem, TimeSpan? expiration = null)
        {
            if (_cache.TryGetValue(key, out T cachedItem)) return cachedItem;
            var newItem = await createItem();
            var defaultCacheExpireTime = _configuration["AppSettings:DefaultCacheExpire"].ToIntNullSafe();
            var defaultCacheExpire = defaultCacheExpireTime == 0 ? 12 : defaultCacheExpireTime;
            await SetAsync(key, newItem, expiration ?? TimeSpan.FromHours(defaultCacheExpire));
            return newItem;
        }

        public async Task ValidateCacheAsync()
        {
            foreach (var (cacheType, filePath) in _cacheFiles)
            {
                if (File.Exists(filePath))
                {
                    var lastWriteTime = File.GetLastWriteTimeUtc(filePath);
                    var cachedTime = await GetAsync<DateTime>(cacheType.ToString());
                    if (lastWriteTime > cachedTime)
                    {
                        await ClearAsync(cacheType);
                        await SetAsync(cacheType.ToString(), lastWriteTime, TimeSpan.FromDays(1));
                    }
                }
            }
        }

        public async Task InvalidateCacheAsync(CacheTypes cacheType)
        {
            if (_cacheFiles.TryGetValue(cacheType, out var filePath))
                await File.WriteAllTextAsync(filePath, DateTime.UtcNow.ToString("O"));
        }

        public async Task ClearAllCacheAsync()
        {
            if (_cache.TryGetValue(_cacheKeys, out List<string> cacheKeyList))
            {
                foreach (var key in cacheKeyList)
                {
                    _cache.Remove(key);
                }
                _cache.Remove(_cacheKeys);
            }

            foreach (var filePath in _cacheFiles.Values)
            {
                await File.WriteAllTextAsync(filePath, DateTime.UtcNow.ToString("O"));
            }
        }

        private void EnsureCacheFilesExist()
        {
            foreach (var filePath in _cacheFiles.Values)
            {
                if (!File.Exists(filePath))
                    File.WriteAllText(filePath, DateTime.UtcNow.ToString("O"));
            }
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
            var cacheSnapshot = new Dictionary<string, object>();

            if (_cache.TryGetValue(_cacheKeys, out List<string> cacheKeyList))
            {
                foreach (var key in cacheKeyList)
                {
                    if (_cache.TryGetValue(key, out object value))
                    {
                        cacheSnapshot[key] = value;
                    }
                }
            }
            var json = JsonConvert.SerializeObject(cacheSnapshot, Formatting.Indented);
            return json
            .Replace("\r", "")
            .Replace("\n", "")
            .Replace("\t", "").Replace("\"", "").Replace("\\r", "").Replace("\\n", "");
        }
    }

    public enum CacheTypes
    {
        Full,
        Agency,
        Location,
        Vendor,
        Markup,
        Configuration,
        VehicleVendor
    }
}
