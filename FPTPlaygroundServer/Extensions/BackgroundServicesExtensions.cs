using FPTPlaygroundServer.Services.Background.AccountVerifies;
using FPTPlaygroundServer.Services.Background.Chats;
using FPTPlaygroundServer.Services.Background.DailyCheckpoints;
using FPTPlaygroundServer.Services.Background.FaceValues;

namespace FPTPlaygroundServer.Extensions;

public static class BackgroundServicesExtensions
{
    public static void AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<AccountVerifyStatusCheckService>();
        services.AddHostedService<AccountVerifyCleanupService>();
        services.AddHostedService<DailyCheckpointRefreshService>();
        services.AddHostedService<FaceValueRefreshQuantityService>();
        services.AddHostedService<MessageBackgroundService>();
    }
}
