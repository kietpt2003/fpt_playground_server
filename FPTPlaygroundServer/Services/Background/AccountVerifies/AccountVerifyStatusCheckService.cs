using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Data;
using Microsoft.EntityFrameworkCore;

namespace FPTPlaygroundServer.Services.Background.AccountVerifies;

public class AccountVerifyStatusCheckService(IServiceProvider serviceProvider) : BackgroundService
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
                    .Where(a => a.CreatedAt.AddMinutes(5) < DateTime.UtcNow && a.VerifyStatus == VerifyStatus.Pending)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(a => a.VerifyStatus, VerifyStatus.Expired), stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(60 * 30), stoppingToken);
        }
    }
}
