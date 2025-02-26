using System.ComponentModel.DataAnnotations.Schema;

namespace FPTPlaygroundServer.Data.Entities;

public class Friendship
{
    public Guid Id { get; set; }
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }
    [ForeignKey(nameof(Friend))]
    public Guid FriendId { get; set; }
    public FriendshipStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    [ForeignKey(nameof(UpdatedUser))]
    public Guid UpdatedBy { get; set; }

    public User User { get; set; } = default!;
    public User Friend { get; set; } = default!;
    public User UpdatedUser { get; set; } = default!;
}

public enum FriendshipStatus
{
    Pending, Accepted, Blocked, Cancelled, Unblocked
}
