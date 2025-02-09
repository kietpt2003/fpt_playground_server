using System.Text.Json;

namespace FPTPlaygroundServer.Common.Utils;

public class JsonSerializerUtils
{
    public static JsonSerializerOptions GetGlobalJsonSerializerOptions()
    {
        return new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
    }

    public static string Serialize<TValue>(TValue value, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(value, options ?? GetGlobalJsonSerializerOptions());
    }

    public static TValue? Deserialize<TValue>(string json, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize<TValue>(json, options ?? GetGlobalJsonSerializerOptions());
    }
}
