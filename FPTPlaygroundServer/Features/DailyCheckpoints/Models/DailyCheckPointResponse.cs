using FPTPlaygroundServer.Data.Entities;

namespace FPTPlaygroundServer.Features.DailyCheckpoints.Models;

public class DailyCheckPointResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int? CoinValue { get; set; }
    public int? DiamondValue { get; set; }
    public DateTime CheckInDate { get; set; }
    public DailyCheckpointStatus Status { get; set; }
}
