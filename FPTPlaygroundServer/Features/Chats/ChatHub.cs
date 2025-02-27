using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Services.Redis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace FPTPlaygroundServer.Features.Chats;

[Authorize]
public class ChatHub(RedisService redisGetSetService) : Hub
{
    private readonly RedisService _redisGetSetService = redisGetSetService;

    // Method to add user to a specific group
    [Authorize("RoleRestricted")]
    public async Task JoinGroup(Guid conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
    }

    public async Task SendMessage(Guid? senderId, Guid? maskedSenderId, Guid? receiverId, Guid conversationId, string content) //content can be image url or message
    {
        var chatMessage = new Message
        {
            ConversationId = conversationId,
            SenderId = senderId,
            UserMaskedId = maskedSenderId,
            Content = content,
            Type = IsImageUrl(content) ? MessageType.Image : MessageType.Text,
            CreatedAt = DateTime.UtcNow,
        };

        // Lưu vào Redis Queue
        await _redisGetSetService.SaveMessageToQueueAsync(chatMessage);

        // Gửi ngay qua SignalR
        await Clients.User(receiverId.ToString()!).SendAsync("PersonalMethod", chatMessage);
    }

    public async Task SendMessageToGroup(Guid? senderId, Guid? maskedSenderId, Guid conversationId, string content) //content can be image url or message
    {
        var chatMessage = new Message
        {
            ConversationId = conversationId,
            SenderId = senderId,
            UserMaskedId = maskedSenderId,
            Content = content,
            Type = IsImageUrl(content) ? MessageType.Image : MessageType.Text,
            CreatedAt = DateTime.UtcNow,
        };

        // Lưu vào Redis Queue
        await _redisGetSetService.SaveMessageToQueueAsync(chatMessage);

        // Gửi ngay qua SignalR
        await Clients.Group(conversationId.ToString()).SendAsync("GroupMethod", chatMessage);
    }

    private static bool IsImageUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;

        string pattern = @"^(https?:\/\/[^\s]+?\.(jpeg|jpg|png))($|\?[^\s]*)$";
        return Regex.IsMatch(url, pattern, RegexOptions.IgnoreCase);
    }
}
