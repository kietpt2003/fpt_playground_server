using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Services.Redis;

namespace FPTPlaygroundServer.Services.Background.Chats;

public class SaveMessageService(IServiceProvider serviceProvider) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var redisService = scope.ServiceProvider.GetRequiredService<RedisService>();

                var message = await redisService.GetNextMessageAsync(); //Save message nhận được (Chỉ save đúng 1 lần, server đầu tiên nhận được sẽ save)

                if (message != null)
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    if (message is not null)
                    {
                        dbContext.Messages.Add(message);
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }

            await Task.Delay(1000, stoppingToken); // Chờ 1s trước khi tiếp tục xử lý
        }
    }
}
