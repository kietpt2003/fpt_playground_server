using Microsoft.AspNetCore.Mvc;

namespace FPTPlaygroundServer.Extensions;

public static class ApiBehaviorExtensions
{
    public static void AddConfigureApiBehavior(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(o =>
        {
            o.SuppressModelStateInvalidFilter = true;
        });
    }
}
