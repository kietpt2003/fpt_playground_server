namespace FPTPlaygroundServer.Extensions;

public static class RedisExtensions
{
    public static void AddStackExchangeRedisCacheForRedis(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "FPTPlayground_";
        });
    }
}
