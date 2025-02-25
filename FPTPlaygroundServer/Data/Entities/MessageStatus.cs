namespace FPTPlaygroundServer.Data.Entities;

public class MessageStatus
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public Guid? ReceiverId { get; set; }
    public Guid? UserMaskedId { get; set; }
    public DateTime? ReadAt { get; set; }

    public Message Message { get; set; } = default!;
    public User Receiver { get; set; } = default!;
    public UserMasked UserMasked { get; set; } = default!;
}
