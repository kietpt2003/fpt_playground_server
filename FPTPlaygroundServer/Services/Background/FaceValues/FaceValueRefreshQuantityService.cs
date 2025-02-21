using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Data;
using Microsoft.EntityFrameworkCore;

namespace FPTPlaygroundServer.Services.Background.FaceValues;

public class FaceValueRefreshQuantityService(IServiceProvider serviceProvider) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            DateTime now = DateTime.UtcNow;
            DateTime nextDay = now.Date.AddDays(1).AddHours(7);
            TimeSpan delay = nextDay - now;

            // Chờ đến 00:00:00 của ngày mới trước khi thực thi
            await Task.Delay(delay, stoppingToken);

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var faceValues = await context.FaceValues
                .Where(fv => fv.Status == FaceValueStatus.Active && fv.Quantity == 0)
                .ToListAsync(stoppingToken);

            foreach (var faceValue in faceValues)
            {
                faceValue.Quantity = faceValue.TotalQuantity;
            }
            await context.SaveChangesAsync(stoppingToken);
        }
    }
}
