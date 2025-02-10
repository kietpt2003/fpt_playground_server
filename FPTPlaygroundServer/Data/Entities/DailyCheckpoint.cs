namespace FPTPlaygroundServer.Data.Entities;

public class DailyCheckpoint
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int? CoinValue { get; set; }
    public int? DiamondValue { get; set; }
    public DateTime CheckInDate { get; set; }
    public DailyCheckpointStatus Status { get; set; }

    public User User { get; set; } = default!;
}

public enum DailyCheckpointStatus
{
    Checked, Unchecked
}
