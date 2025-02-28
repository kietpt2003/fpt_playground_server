using System.ComponentModel.DataAnnotations.Schema;

namespace FPTPlaygroundServer.Data.Entities;

public class Notification
{
    public Guid Id { get; set; }
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }
    public string? Title { get; set; }
    public string Content { get; set; } = default!;
    public bool IsRead { get; set; }
    public NotificationType Type { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = default!;
}

public enum NotificationType
{
    Deposit, Friendship, Mate
}
