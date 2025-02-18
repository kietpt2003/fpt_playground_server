using FPTPlaygroundServer.Services.Background.AccountVerifies;
using FPTPlaygroundServer.Services.Background.DailyCheckpoints;

namespace FPTPlaygroundServer.Extensions;

public static class BackgroundServicesExtensions
{
    public static void AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<AccountVerifyStatusCheckService>();
        services.AddHostedService<AccountVerifyCleanupService>();
        services.AddHostedService<DailyCheckpointRefreshService>();
    }
}
