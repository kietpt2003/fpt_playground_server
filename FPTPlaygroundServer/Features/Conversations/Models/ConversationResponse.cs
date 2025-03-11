using FPTPlaygroundServer.Data.Entities;

namespace FPTPlaygroundServer.Features.Conversations.Models;

public class ConversationResponse
{
    public Guid Id { get; set; }
    public int? ConversationIndex { get; set; }
    public string Name { get; set; } = default!;
    public ConversationType Type { get; set; }
    public string? GroupImageUrl { get; set; }
    public ConversationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsJoined { get; set; }
}
