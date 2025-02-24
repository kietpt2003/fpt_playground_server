namespace FPTPlaygroundServer.Common.Settings;

public class RedisSettings
{
    public static readonly string Section = "Redis";
    public string ConnectionString { get; set; } = default!;
}
