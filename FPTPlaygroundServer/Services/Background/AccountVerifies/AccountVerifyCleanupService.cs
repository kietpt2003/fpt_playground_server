using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Data;
using Microsoft.EntityFrameworkCore;

namespace FPTPlaygroundServer.Services.Background.AccountVerifies;

public class AccountVerifyCleanupService(IServiceProvider serviceProvider): BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                await context.AccountVerifies
                    .Where(a => a.VerifyStatus == VerifyStatus.Expired)
                    .ExecuteDeleteAsync(stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(60 * 30), stoppingToken);
        }
    }
}
