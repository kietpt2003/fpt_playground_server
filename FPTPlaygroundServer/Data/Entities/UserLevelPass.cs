namespace FPTPlaygroundServer.Data.Entities;

public class UserLevelPass
{
    public Guid UserId { get; set; }
    public Guid LevelPassId { get; set; }
    public int Experience { get; set; }
    public bool IsClaim { get; set; }

    public User User { get; set; } = default!;
    public LevelPass LevelPass { get; set; } = default!;
}
