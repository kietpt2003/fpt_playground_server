using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using FPTPlaygroundServer.Services.Auth.Models;

namespace FPTPlaygroundServer.Services.Chats;

public class ChatService : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
    {
        // Lấy UserId từ Claim "UserInfo" (hoặc bất kỳ claim nào bạn muốn)
        var userInfoJson = connection.User?.Claims.FirstOrDefault(c => c.Type == "UserInfo")?.Value;
        var userInfo = JsonConvert.DeserializeObject<TokenRequest>(userInfoJson!);

        // Trả về UserId làm UserIdentifier
        return userInfo?.UserId.ToString()!;
    }
}
