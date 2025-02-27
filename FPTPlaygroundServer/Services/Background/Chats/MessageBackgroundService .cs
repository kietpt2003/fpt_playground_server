using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Features.Chats;
using FPTPlaygroundServer.Services.Redis;
using Microsoft.AspNetCore.SignalR;

namespace FPTPlaygroundServer.Services.Background.Chats;

public class MessageBackgroundService(IServiceProvider serviceProvider, IHubContext<ChatHub> hub) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IHubContext<ChatHub> _hub = hub;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var redisService = scope.ServiceProvider.GetRequiredService<RedisService>();

                var message = await redisService.GetNextMessageAsync();

                if (message != null)
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    if (message is not null)
                    {
                        await _hub.Clients.Group(message.ConversationId.ToString()).SendAsync("GroupMethod", message.Content, stoppingToken);
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
