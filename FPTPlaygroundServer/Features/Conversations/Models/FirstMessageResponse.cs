using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Features.FriendShips.Models;

namespace FPTPlaygroundServer.Features.Conversations.Models;

public class FirstMessageResponse
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public FriendResponse? Sender { get; set; }
    public UserMaskedResponse? UserMasked { get; set; }
    public string Content { get; set; } = default!;
    public MessageType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
