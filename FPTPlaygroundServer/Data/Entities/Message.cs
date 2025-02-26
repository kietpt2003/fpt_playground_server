namespace FPTPlaygroundServer.Data.Entities;

public class Message
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid? SenderId { get; set; }
    public Guid? UserMaskedId { get; set; }
    public string Content { get; set; } = default!;
    public MessageType Type { get; set; }
    public DateTime CreatedAt { get; set; }

    public Conversation Conversation { get; set; } = default!;
    public User Sender { get; set; } = default!;
    public UserMasked UserMasked { get; set; } = default!;
    public ICollection<MessageStatus> MessageStatuses { get; set; } = [];
}

public enum MessageType
{
    Text, Image, System
}
