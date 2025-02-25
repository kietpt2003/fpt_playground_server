namespace FPTPlaygroundServer.Data.Entities;

public class ConversationMember
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? UserMaskedId { get; set; }
    public ConversationMemberRole Role { get; set; }
    public ConversationMemberStatus Status { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Conversation Conversation { get; set; } = default!;
    public User? User { get; set; }
    public UserMasked? UserMasked { get; set; }
}

public enum ConversationMemberStatus
{
    Joined, Outed, Kicked
}

public enum ConversationMemberRole
{
    Owner, Member
}
