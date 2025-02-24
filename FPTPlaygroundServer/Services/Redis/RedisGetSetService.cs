using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace FPTPlaygroundServer.Services.Redis;

public class RedisGetSetService(IDistributedCache cache)
{
    private readonly IDistributedCache _cache = cache;
    public async Task SetRecordAsync<T>(string recordId,
        T data,
        TimeSpan? absoluteExpireTime = null,
        TimeSpan? unusedExpireTime = null)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromSeconds(60),
            SlidingExpiration = unusedExpireTime,
        };

        var jsonData = JsonSerializer.Serialize(data);
        await _cache.SetStringAsync(recordId, jsonData, options);
    }

    public async Task<T> GetRecordAsync<T>(string recordId)
    {
        var jsonData = await _cache.GetStringAsync(recordId);

        if (jsonData is null)
        {
            return default(T)!;
        }

        return JsonSerializer.Deserialize<T>(jsonData)!;
    }
}
