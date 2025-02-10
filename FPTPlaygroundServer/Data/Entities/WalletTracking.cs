using System.ComponentModel.DataAnnotations.Schema;

namespace FPTPlaygroundServer.Data.Entities;

public class WalletTracking
{
    public Guid Id { get; set; }
    [ForeignKey(nameof(CoinWallet))]
    public Guid? CoinWalletId { get; set; }
    [ForeignKey(nameof(DiamondWallet))]
    public Guid? DiamondWalletId { get; set; }
    public Guid FaceValueId { get; set; }
    public WalletTrackingPaymentMethod PaymentMethod { get; set; }
    public int Amount { get; set; }
    public WalletTrackingType Type { get; set; }
    public WalletTrackingStatus Status { get; set; }
    public long PaymentCode { get; set; }
    public DateTime? DepositedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public CoinWallet? CoinWallet { get; set; }
    public DiamondWallet? DiamondWallet { get; set; }
    public FaceValue FaceValue { get; set; } = default!;
}

public enum WalletTrackingPaymentMethod
{
    PayOS
}

public enum WalletTrackingType
{
    Deposit, TransferCoin, TransferDiamond
}

public enum WalletTrackingStatus
{
    Pending, Success, Cancelled
}
