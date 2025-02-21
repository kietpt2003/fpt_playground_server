using FPTPlaygroundServer.Data.Entities;

namespace FPTPlaygroundServer.Features.FaceValues.Models;

public class FaceValueResponse
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
}
