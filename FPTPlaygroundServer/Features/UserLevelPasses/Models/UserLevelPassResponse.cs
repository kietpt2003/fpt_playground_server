namespace FPTPlaygroundServer.Features.UserLevelPasses.Models;

public class UserLevelPassResponse
{
    public Guid UserId { get; set; }
    public Guid LevelPassId { get; set; }
    public int Level { get; set; }
    public int? CoinValue { get; set; }
    public int? DiamondValue { get; set; }
    public int Experience { get; set; }
    public int Percentage { get; set; }
    public int Require { get; set; }
    public bool IsClaim { get; set; }
}
