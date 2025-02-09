using System.ComponentModel.DataAnnotations.Schema;

namespace FPTPlaygroundServer.Data.Entities;

public class Friendship
{
    public Guid Id { get; set; }
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }
    [ForeignKey(nameof(Friend))]
    public Guid FriendId { get; set; }
    public FriendShipStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = default!;
    public User Friend { get; set; } = default!;
}

public enum FriendShipStatus
{
    Pending, Accepted, Blocked
}
