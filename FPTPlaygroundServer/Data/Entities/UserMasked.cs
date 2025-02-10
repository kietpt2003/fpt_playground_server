namespace FPTPlaygroundServer.Data.Entities;

public class UserMasked
{
    public Guid Id { get; set; }
    public Guid MaskedAvatarId { get; set; }
    public Guid UserId { get; set; }
    public UserMaskedStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public MaskedAvatar MaskedAvatar { get; set; } = default!;
    public User User { get; set; } = default!;
    public ICollection<ConversationMember> ConversationMembers { get; set; } = [];
    public ICollection<Message> Messages { get; set; } = [];
    public ICollection<MessageStatus> MessageStatuses { get; set; } = [];
}

public enum UserMaskedStatus
{
    Active, Inactive
}
