namespace FPTPlaygroundServer.Data.Entities;

public class FaceValue
{
    public Guid Id { get; set; }
    public int CoinValue { get; set; }
    public int DiamondValue { get; set; }
    public int VNDValue { get; set; }
    public int Quantity { get; set; }
    public FaceValueStatus Status { get; set; }
    public DateTime StartedDate { get; set; }
    public DateTime? EndedDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<WalletTracking> WalletTrackings { get; set; } = [];
}

public enum FaceValueStatus
{
    Active, Inactive
}
