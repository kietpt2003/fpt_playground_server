namespace FPTPlaygroundServer.Data.Entities;

public class UserAchievement
{
    public Guid UserId { get; set; }
    public Guid AchievementId { get; set; }
    public int Progress { get; set; }

    public User User { get; set; } = default!;
    public Achievement Achievement { get; set; } = default!;
}
