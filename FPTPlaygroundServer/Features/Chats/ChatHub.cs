using FPTPlaygroundServer.Common.Settings;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Services.Redis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.RegularExpressions;
using FPTPlaygroundServer.Services.Auth.Models;

namespace FPTPlaygroundServer.Features.Chats;

[Authorize]
public class ChatHub(RedisService redisGetSetService, AppDbContext context, IOptions<JwtSettings> jwtSettings) : Hub
{
    private readonly RedisService _redisGetSetService = redisGetSetService;
    private readonly AppDbContext _context = context;
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly SymmetricSecurityKey _key = new(Encoding.UTF8.GetBytes(jwtSettings.Value.SigningKey));

    // Method to add user to a specific group
    [Authorize("RoleRestricted")]
    public async Task JoinGroup(Guid conversationId)
    {
        var httpContext = Context.GetHttpContext() ?? throw new HubException("Cannot access HttpContext");
        var accessToken = httpContext.Request.Query["access_token"];
        if (string.IsNullOrEmpty(accessToken))
        {
            throw new HubException("Access Token Required.");
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = _key,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            ClockSkew = TimeSpan.Zero
        };

        var principal = tokenHandler.ValidateToken(accessToken, validationParameters, out SecurityToken validatedToken);
        var userInfoJson = principal.Claims.FirstOrDefault(c => c.Type == "UserInfo")?.Value;

        if (string.IsNullOrEmpty(userInfoJson))
            throw new HubException("Don't have user info in Token.");

        var desrializeSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Include,
            ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver()
        };

        var jObject = JObject.Parse(userInfoJson);

        if (jObject["UserId"]?.Type == JTokenType.Null)
        {
            throw new HubException("Don't have user info in Token.");
        }

        var tokenInfo = new TokenRequest
        {
            UserId = jObject["UserId"]?.ToObject<Guid?>(),
            Email = jObject["Email"]!.ToString(),
            Role = Enum.TryParse(jObject["Role"]?.ToString(), out Role role) ? role : Role.User
        };

        var user = await _context.Users
            .Include(u => u.Account)
                .ThenInclude(a => a.Devices)
            .Include(u => u.Specialize)
            .Include(u => u.Server)
            .Include(u => u.CoinWallet)
            .Include(u => u.DiamondWallet)
            .FirstOrDefaultAsync(x => x.Id == tokenInfo!.UserId) ?? throw new HubException("User not found");

        var conversation = await _context.Conversations
            .Include(c => c.ConversationMembers)
                    .ThenInclude(cm => cm.UserMasked)
            .FirstOrDefaultAsync(c => c.Id == conversationId) ?? throw new HubException("Conversation not found");

        bool isJoined = conversation.ConversationMembers.Any(cm => (cm.UserId.HasValue && cm.UserId == user.Id) || (cm.UserMaskedId.HasValue && cm.UserMasked!.UserId == user.Id));

        if (isJoined)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
        } else
        {
            throw new HubException("Cannot join this conversation");
        }
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

        // Lưu vào Redis Queue, để background task save vào DB
        await _redisGetSetService.SaveMessageToQueueAsync(chatMessage);

        // Gửi ngay qua Redis Pub
        await _redisGetSetService.PublishMessage(chatMessage);
    }

    private static bool IsImageUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;

        string pattern = @"^(https?:\/\/[^\s]+?\.(jpeg|jpg|png))($|\?[^\s]*)$";
        return Regex.IsMatch(url, pattern, RegexOptions.IgnoreCase);
    }
}
