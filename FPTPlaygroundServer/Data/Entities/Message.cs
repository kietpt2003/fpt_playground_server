using System.ComponentModel.DataAnnotations.Schema;

namespace FPTPlaygroundServer.Data.Entities;

public class Message
{
    public Guid Id { get; set; }
    [ForeignKey(nameof(Parent))]
    public Guid? ParentId { get; set; }
    public Guid ConversationId { get; set; }
    public Guid? SenderId { get; set; }
    public Guid? UserMaskedId { get; set; }
    public string Content { get; set; } = default!;
    public MessageType Type { get; set; }
    public DateTime CreatedAt { get; set; }

    public Message Parent { get; set; } = default!;
    public Conversation Conversation { get; set; } = default!;
    public User Sender { get; set; } = default!;
    public UserMasked UserMasked { get; set; } = default!;
    public ICollection<MessageStatus> MessageStatuses { get; set; } = [];
}

public enum MessageType
{
    Text, Image, System
}
