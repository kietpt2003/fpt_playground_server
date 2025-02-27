using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Features.Chats;
using FPTPlaygroundServer.Services.Redis.Models;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using System.Text.Json;

namespace FPTPlaygroundServer.Services.Background.Chats;

public class SendMessageService(IConnectionMultiplexer redis, IHubContext<ChatHub> hub) : BackgroundService
{
    private readonly IHubContext<ChatHub> _hub = hub;
    private readonly IConnectionMultiplexer _redis = redis;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var subscriber = _redis.GetSubscriber();
                await subscriber.SubscribeAsync(RedisChannels.ChatChannel.ToString(), async (channel, message) =>
                {
                    // Khi nhận tin nhắn từ Redis, deserialize và phát tin nhắn qua SignalR
                    var chatMessage = JsonSerializer.Deserialize<Message>(message);
                    if (chatMessage != null)
                    {
                        await _hub.Clients.Group(chatMessage.ConversationId.ToString())
                            .SendAsync("GroupMethod", chatMessage, stoppingToken);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }

            await Task.Delay(1000, stoppingToken); // Chờ 1s trước khi tiếp tục xử lý
        }
    }
}
