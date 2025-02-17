namespace FPTPlaygroundServer.Data.Entities;

public class LevelPass
{
    public Guid Id { get; set; }
    public int Level { get; set; }
    public int? CoinValue { get; set; }
    public int? DiamondValue { get; set; }
    public int Require { get; set; }
    public LevelPassStatus Status { get; set; }

    public ICollection<UserLevelPass> UserLevelPasses { get; set; } = [];
}

public enum LevelPassStatus
{
    Active, Inactive
}
