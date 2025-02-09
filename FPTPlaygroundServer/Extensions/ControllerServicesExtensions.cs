using System.Text.Json.Serialization;

namespace FPTPlaygroundServer.Extensions;

public static class ControllerServicesExtensions
{
    public static void AddControllerServices(this IServiceCollection services)
    {
        services.AddControllers(o =>
        {
            o.UseRoutePrefix("api");
        }).AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
    }
}
