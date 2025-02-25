namespace FPTPlaygroundServer.Data.Entities;

public class Conversation
{
    public Guid Id { get; set; }
    public int? ConversationIndex { get; set; }
    public string Name { get; set; } = default!;
    public ConversationType Type { get; set; }
    public string? GroupImageUrl { get; set; }
    public ConversationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ConversationMember> ConversationMembers { get; set; } = [];
    public ICollection<Message> Messages { get; set; } = [];
}

public enum ConversationType
{
    StudyGroup, DatingGroup, CuriousGroup, Personal, Dating
}

public enum ConversationStatus
{
    Active, Inactive
}
