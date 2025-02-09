using System.ComponentModel.DataAnnotations.Schema;

namespace FPTPlaygroundServer.Data.Entities;

public class Mate
{
    public Guid Id { get; set; }
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }
    [ForeignKey(nameof(YourMate))]
    public Guid MateId { get; set; }
    public MateStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = default!;
    public User YourMate { get; set; } = default!;
}

public enum MateStatus
{
    Pending, Dated
}
