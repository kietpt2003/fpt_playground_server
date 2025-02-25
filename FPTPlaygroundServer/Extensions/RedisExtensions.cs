using StackExchange.Redis;

namespace FPTPlaygroundServer.Extensions;

public static class RedisExtensions
{
    //public static void AddStackExchangeRedisCacheForRedis(this IServiceCollection services, IConfiguration configuration) //Just use for caching and cannot message broker
    //{
    //    services.AddStackExchangeRedisCache(options =>
    //    {
    //        options.Configuration = configuration.GetConnectionString("Redis");
    //        options.InstanceName = "FPTPlayground_";
    //    });
    //}

    public static void ConnectionMultiplexerForRedis(this IServiceCollection services, IConfiguration configuration) //Connect Redis just 1 times for message broker and caching
    {
        var multiplexer = ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!);
        services.AddSingleton<IConnectionMultiplexer>(multiplexer);

        //Ké kết nối của Multiplexer để caching
        services.AddStackExchangeRedisCache(options =>
        {
            options.ConnectionMultiplexerFactory = () => Task.FromResult<IConnectionMultiplexer>(multiplexer);
            options.InstanceName = "FPTPlayground_";
        });
    }
}
