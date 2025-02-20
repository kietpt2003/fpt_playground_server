using Net.payOS.Types;

namespace FPTPlaygroundServer.Services.Payment.Models;

public class PayOSPaymentRequest
{
    public long OrderCode { get; set; }
    public int Amount { get; set; }
    public string Description { get; set; } = default!;

    public List<ItemData> Items { get; set; } = [];

    public string CancelUrl { get; set; } = default!;

    public string ReturnUrl { get; set; } = default!;
}
