using Net.payOS.Types;

namespace FPTPlaygroundServer.Services.Payment.Models;

public class PayOSPayment
{
    public long PaymentReferenceId { get; set; } = default!;
    public int Amount { get; set; }
    public string? Info { get; set; }
    public List<ItemData> PaymentItems { get; set; } = [];
    public string ReturnUrl { get; set; } = default!;
}
