using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Features.FriendShips.Models;

namespace FPTPlaygroundServer.Features.Conversations.Models;

public class UserConversationResponse
{
    public Guid Id { get; set; }
    public FriendResponse? Friend { get; set; }
    public UserMaskedResponse? UserMasked { get; set; }
    public ConversationType Type { get; set; }
    public bool IsBlocked { get; set; }
    public Guid? IsBlockedBy { get; set; }
    public FirstMessageResponse? FirstMessage { get; set; }
    public ConversationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
