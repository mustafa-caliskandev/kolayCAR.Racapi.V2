using System;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services.Abstract;

public interface ICacheService
{
    //void Set<T>(string key, T data, TimeSpan? absoluteExpiration = null);
    //T Get<T>(string key);
    //void Remove(string key);
    //void Clear(CacheTypes cacheType);
    //Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> createItem, TimeSpan? expiration = null);
    //bool UseCache();
    //void ValidateCache();
    //void InvalidateCache(CacheTypes cacheType);
    //string GetAllCacheAsJson();

    Task SetAsync<T>(string key, T data, TimeSpan? absoluteExpiration = null);
    Task<T> GetAsync<T>(string key);
    Task RemoveAsync(string key);
    Task ClearAsync(CacheTypes cacheType);
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> createItem, TimeSpan? expiration = null);
    Task ValidateCacheAsync();
    Task InvalidateCacheAsync(CacheTypes cacheType);
    Task ClearAllCacheAsync();
    Task<string> GetAllCacheAsJsonAsync();
    Task RemoveByPrefixAsync(string prefixKey);
}
