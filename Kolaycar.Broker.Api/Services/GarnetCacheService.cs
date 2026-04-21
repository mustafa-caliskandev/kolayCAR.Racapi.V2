using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.API.Services.Abstract;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class GarnetCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    private const string CacheKeysSet = "cache:keys";
    private static readonly string BrokerName = CacheSettings.BrokerName;
    private readonly TimeSpan _defaultExpire = TimeSpan.FromMinutes(30);

    public GarnetCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = _redis.GetDatabase();
    }
    public async Task SetAsync<T>(string key, T data, TimeSpan? absoluteExpiration = null)
    {
        if (data == null) return;

        var json = JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });
        var expire = absoluteExpiration ?? TimeSpan.FromMinutes(10);

        await _db.StringSetAsync($"{BrokerName}-{key}", json, expire);
        await _db.SetAddAsync(CacheKeysSet, $"{BrokerName}-{key}");
    }

    public async Task<T> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync($"{BrokerName}-{key}");
        if (!value.HasValue) return default;

        return JsonConvert.DeserializeObject<T>(value);
    }

    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync($"{BrokerName}-{key}");
        await _db.SetRemoveAsync(CacheKeysSet, $"{BrokerName}-{key}");
    }

    public async Task RemoveByPrefixAsync(string prefixKey)
    {
        var keys = await _db.SetMembersAsync(CacheKeysSet);
        var searchPrefix = $"{BrokerName}-{prefixKey}";
        foreach (var redisValue in keys)
        {
            var key = redisValue.ToString();
            if (key.StartsWith(searchPrefix))
            {
                await _db.KeyDeleteAsync(key);
                await _db.SetRemoveAsync(CacheKeysSet, key);
            }
        }
    }
    public async Task ClearAsync(CacheTypes cacheType)
    {
        var prefix = GetPrefix(cacheType);

        var keys = await _db.SetMembersAsync(CacheKeysSet);

        foreach (var redisValue in keys)
        {
            var key = redisValue.ToString();

            if (key.StartsWith(prefix))
            {
                await _db.KeyDeleteAsync(key);
                await _db.SetRemoveAsync(CacheKeysSet, key);
            }
        }
    }

    public Task InvalidateCacheAsync(CacheTypes cacheType)
    {
        return ClearAsync(cacheType);
    }

    public async Task ClearAllCacheAsync()
    {
        var keys = await _db.SetMembersAsync(CacheKeysSet);
        foreach (var redisValue in keys)
        {
            await _db.KeyDeleteAsync(redisValue.ToString());
        }
        await _db.KeyDeleteAsync(CacheKeysSet);
    }
    public async Task<T> GetOrCreateAsync2<T>(
        string key,
        Func<Task<T>> createItem,
        TimeSpan? expiration = null)
    {
        var cached = await GetAsync<T>(key);
        if (cached != null && !cached.Equals(default(T)))
            return cached;

        var data = await createItem();

        await SetAsync(key, data, expiration);
        return data;
    }
    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> createItem, TimeSpan? expiration = null)
    {
        var cached = await _db.StringGetAsync($"{BrokerName}-{key}");

        if (cached.HasValue)
            return JsonConvert.DeserializeObject<T>(cached);

        var lockTaken = await _db.StringSetAsync($"{BrokerName}-{key}-lock", "1", TimeSpan.FromSeconds(10), When.NotExists);

        if (lockTaken)
        {
            try
            {
                cached = await _db.StringGetAsync($"{BrokerName}-{key}");
                if (cached.HasValue)
                    return JsonConvert.DeserializeObject<T>(cached);

                var data = await createItem();

                var json = JsonConvert.SerializeObject(data, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

                await _db.StringSetAsync($"{BrokerName}-{key}", json, expiration ?? _defaultExpire);
                await _db.SetAddAsync(CacheKeysSet, $"{BrokerName}-{key}");

                return data;
            }
            finally
            {
                await _db.KeyDeleteAsync($"{BrokerName}-{key}-lock");
            }
        }
        else
        {
            await Task.Delay(70);

            cached = await _db.StringGetAsync($"{BrokerName}-{key}");
            if (cached.HasValue)
                return JsonConvert.DeserializeObject<T>(cached);

            var data = await createItem();

            var json = JsonConvert.SerializeObject(data);
            await _db.StringSetAsync($"{BrokerName}-{key}", json, expiration ?? _defaultExpire);
            await _db.SetAddAsync(CacheKeysSet, $"{BrokerName}-{key}");

            return data;
        }
    }

    public async Task<string> GetAllCacheAsJsonAsync()
    {
        var snapshot = new Dictionary<string, object>();
        var keys = await _db.SetMembersAsync(CacheKeysSet);

        foreach (var redisValue in keys)
        {
            var key = redisValue.ToString();
            var value = await _db.StringGetAsync($"{BrokerName}-{key}");

            if (value.HasValue)
            {
                snapshot[key] =
                    JsonConvert.DeserializeObject<object>(value);
            }
        }

        return JsonConvert.SerializeObject(snapshot, Formatting.None);
    }

    public Task ValidateCacheAsync() => Task.CompletedTask;

    private string GetPrefix(CacheTypes cacheTypes)
    {
        string prefix;

        switch (cacheTypes)
        {
            case CacheTypes.Full:
                prefix = CacheSettings.BrokerName;
                break;
            case CacheTypes.Agency:
                prefix = $"{CacheSettings.BrokerName}-{CacheSettings.AgencyKey}";
                break;
            case CacheTypes.Location:
                prefix = $"{CacheSettings.BrokerName}-{CacheSettings.LocationKey}";
                break;
            case CacheTypes.Vendor:
                prefix = $"{CacheSettings.BrokerName}-{CacheSettings.VendorKey}";
                break;
            case CacheTypes.Markup:
                prefix = $"{CacheSettings.BrokerName}-{CacheSettings.MarkupKey}";
                break;
            case CacheTypes.Configuration:
                prefix = $"{CacheSettings.BrokerName}-{CacheSettings.ConfigurationKey}";
                break;
            case CacheTypes.VehicleVendor:
                prefix = $"{CacheSettings.BrokerName}-{CacheSettings.VehicleVendorKey}";
                break;
            default:
                prefix = CacheSettings.BrokerName;
                break;
        }

        return prefix;
    }
}
