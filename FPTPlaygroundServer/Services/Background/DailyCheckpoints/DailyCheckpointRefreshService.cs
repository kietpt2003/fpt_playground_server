using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Data;
using Microsoft.EntityFrameworkCore;

namespace FPTPlaygroundServer.Services.Background.DailyCheckpoints;

public class DailyCheckpointRefreshService(IServiceProvider serviceProvider) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            DateTime now = DateTime.UtcNow;
            DateTime nextMonday = GetNextMondayAtMidnight(now);
            TimeSpan delay = nextMonday - now;

            await Task.Delay(delay, stoppingToken);

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            List<DailyCheckpoint> checkpointList = [];
            for (int i = 0; i < 7; i++) //Tạo Checkpoint cho tuần tới (Tuần mà sau của tuần sau)
            {
                DateTime checkpoint = nextMonday.AddDays(i + 7);

                var users = await context.Users
                    .Include(u => u.Account)
                    .Where(u => u.Status == UserStatus.Active && u.Account.Status == AccountStatus.Active)
                    .ToListAsync(stoppingToken);

                foreach (var user in users)
                {
                    bool isExist = await context.DailyCheckpoints
                        .AnyAsync(d => d.UserId == user.Id && d.CheckInDate >= checkpoint, stoppingToken);
                    if (isExist) //Nếu tạo rồi thì không tạo nữa
                    {
                        return;
                    }
                    DailyCheckpoint dailyCheckpoint = new()
                    {
                        UserId = user.Id,
                        CoinValue = 200,
                        DiamondValue = i == 6 ? 50 : null,
                        CheckInDate = checkpoint,
                        Status = DailyCheckpointStatus.Unchecked,
                    };
                    checkpointList.Add(dailyCheckpoint);
                }
            }

            if (checkpointList.Count > 0)
            {
                await context.DailyCheckpoints.AddRangeAsync(checkpointList, stoppingToken);
                await context.SaveChangesAsync(stoppingToken);
            }

            //Xóa các Checkpoint của tuần trước
            await context.DailyCheckpoints
                .Where(d => d.CheckInDate < nextMonday.AddDays(-7))
                .ExecuteDeleteAsync(stoppingToken);

            //Cập nhật nextMonday để đảm bảo vòng lặp đợi đến tuần kế tiếp
            now = DateTime.UtcNow;
            nextMonday = GetNextMondayAtMidnight(now);
        }
    }

    private static DateTime GetNextMondayAtMidnight(DateTime now)
    {
        int daysUntilMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
        if (daysUntilMonday == 0 && now.TimeOfDay.TotalSeconds > 0)
        {
            daysUntilMonday = 7; // Nếu hôm nay đã qua 00:00:00, thì phải chờ đến thứ Hai tuần sau
        }
        return now.Date.AddDays(daysUntilMonday);
    }
}
