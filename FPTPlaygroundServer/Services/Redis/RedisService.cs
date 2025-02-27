using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Services.Redis.Models;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;

namespace FPTPlaygroundServer.Services.Redis;

public class RedisService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDistributedCache _cache;
    private readonly IDatabase _database;

    private const string ChatQueueKey = "chat_messages";

    public RedisService(IConnectionMultiplexer redis, IDistributedCache cache)
    {
        _redis = redis;
        _cache = cache;
        _database = _redis.GetDatabase();
    }

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

    public async Task SaveMessageToQueueAsync(Message message)
    {
        string jsonMessage = JsonSerializer.Serialize(message);
        await _database.ListRightPushAsync(ChatQueueKey, jsonMessage);
    }

    public async Task<Message?> GetNextMessageAsync()
    {
        string? jsonMessage = await _database.ListLeftPopAsync(ChatQueueKey);
        return jsonMessage != null ? JsonSerializer.Deserialize<Message>(jsonMessage) : null;
    }

    public async Task PublishMessage(Message message)
    {
        var subscriber = _redis.GetSubscriber();
        string jsonMessage = JsonSerializer.Serialize(message);
        await subscriber.PublishAsync(RedisChannels.ChatChannel.ToString(), jsonMessage);
    }
}
