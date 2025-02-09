using System.ComponentModel.DataAnnotations.Schema;

namespace FPTPlaygroundServer.Data.Entities;

public class Achievement
{
    public Guid Id { get; set; }
    [ForeignKey(nameof(AchievementEntity))]
    public Guid? ParentId { get; set; }
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public int? CoinValue { get; set; }
    public int? DiamondValue { get; set; }
    public AchievementType Type { get; set; }
    public int Require { get; set; }
    public AchievementStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Achievement? AchievementEntity { get; set; }
}

public enum AchievementType
{
    Common, Rare, Epic, Legendary
}

public enum AchievementStatus
{
    Active, Inactive
}
