using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Infrastructure.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConfiguration _configuration;
    private readonly IConnectionMultiplexer _redis;

    private const string CacheKeysSet = "cache:keys";

    public RedisCacheService(
        IDistributedCache cache,
        IConnectionMultiplexer redis,
        IConfiguration configuration)
    {
        _cache = cache;
        _redis = redis;
        _configuration = configuration;
    }

    public async Task SetAsync<T>(string key, T data, TimeSpan? absoluteExpiration = null)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.StringSetAsync("test", "1", TimeSpan.FromMinutes(1));
        }
        catch (Exception ex)
        {
            throw;
        }


        if (data == null || !UseCache()) return;

        var expire = GetDefaultExpire(absoluteExpiration);
        var json = JsonConvert.SerializeObject(data);

        await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expire
        });

        await AddKeyAsync(key);
    }

    public async Task<T> GetAsync<T>(string key)
    {
        if (!UseCache()) return default;

        var json = await _cache.GetStringAsync(key);
        return json == null ? default : JsonConvert.DeserializeObject<T>(json);
    }

    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
        await RemoveKeyAsync(key);
    }

    public async Task RemoveByPrefixAsync(string prefixKey)
    {
        var db = _redis.GetDatabase();
        var keys = await db.SetMembersAsync(CacheKeysSet);

        foreach (var redisKey in keys)
        {
            var key = redisKey.ToString();
            if (key.StartsWith(prefixKey))
            {
                await _cache.RemoveAsync(key);
                await db.SetRemoveAsync(CacheKeysSet, key);
            }
        }
    }

    public async Task<T> GetOrCreateAsync<T>(
    string key,
    Func<Task<T>> createItem,
    TimeSpan? expiration = null)
    {
        if (!UseCache())
            return await createItem();

        var cached = await GetAsync<T>(key);
        if (cached != null)
            return cached;

        var newItem = await createItem();
        await SetAsync(key, newItem, expiration);
        return newItem;
    }
    public async Task ClearAsync(CacheTypes cacheType)
    {
        var db = _redis.GetDatabase();
        var keys = await db.SetMembersAsync(CacheKeysSet);

        foreach (var redisKey in keys)
        {
            var key = redisKey.ToString();
            if (cacheType == CacheTypes.Full || key.Contains(cacheType.ToString()))
            {
                await _cache.RemoveAsync(key);
                await db.SetRemoveAsync(CacheKeysSet, key);
            }
        }
    }

    public bool UseCache()
    => _configuration["AppSettings:UseCache"].ToBoolNullSafe();
    public Task ValidateCacheAsync() => Task.CompletedTask;

    public Task InvalidateCacheAsync(CacheTypes cacheType)
    {
        return ClearAsync(cacheType);
    }

    public async Task ClearAllCacheAsync()
    {
        var db = _redis.GetDatabase();
        var keys = await db.SetMembersAsync(CacheKeysSet);

        foreach (var redisKey in keys)
        {
            var key = redisKey.ToString();
            await _cache.RemoveAsync(key);
        }
        await db.KeyDeleteAsync(CacheKeysSet);
    }
    public async Task<string> GetAllCacheAsJsonAsync()
    {
        var db = _redis.GetDatabase();
        var snapshot = new Dictionary<string, object>();

        var keys = await db.SetMembersAsync(CacheKeysSet);
        foreach (var redisKey in keys)
        {
            var value = await _cache.GetStringAsync(redisKey);
            if (value != null)
                snapshot[redisKey] = JsonConvert.DeserializeObject<object>(value);
        }

        return JsonConvert.SerializeObject(snapshot, Formatting.None);
    }

    private async Task AddKeyAsync(string key)
    {
        await _redis.GetDatabase().SetAddAsync(CacheKeysSet, key);
    }

    private async Task RemoveKeyAsync(string key)
    {
        await _redis.GetDatabase().SetRemoveAsync(CacheKeysSet, key);
    }

    private TimeSpan GetDefaultExpire(TimeSpan? expiration)
    {
        var defaultExpire =
            _configuration["AppSettings:DefaultCacheExpire"].ToIntNullSafe();

        return expiration ??
               TimeSpan.FromHours(defaultExpire == 0 ? 12 : defaultExpire);
    }
}
