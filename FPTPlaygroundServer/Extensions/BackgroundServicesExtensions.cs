using FPTPlaygroundServer.Services.Background.AccountVerifies;

namespace FPTPlaygroundServer.Extensions;

public static class BackgroundServicesExtensions
{
    public static void AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<AccountVerifyStatusCheckService>();
        services.AddHostedService<AccountVerifyCleanupService>();
    }
}
