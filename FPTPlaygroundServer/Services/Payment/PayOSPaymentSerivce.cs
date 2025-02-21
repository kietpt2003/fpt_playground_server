using FPTPlaygroundServer.Common.Settings;
using FPTPlaygroundServer.Services.Server;
using FPTPlaygroundServer.Services.Payment.Models;
using Microsoft.Extensions.Options;
using Net.payOS.Types;
using Net.payOS;

namespace FPTPlaygroundServer.Services.Payment;

public class PayOSPaymentSerivce(IOptions<PayOSSettings> payOSSettings, CurrentServerService currentServerService)
{
    private const string DefaultOrderInfo = "Pay with PayOS";

    private readonly PayOSSettings _payOSSettings = payOSSettings.Value;

    public async Task<string> CreatePaymentAsync(PayOSPayment payment)
    {
        var request = new PayOSPaymentRequest
        {
            OrderCode = payment.PaymentReferenceId,
            Amount = payment.Amount,
            Description = payment.Info ?? DefaultOrderInfo,
            Items = payment.PaymentItems,
            CancelUrl = payment.ReturnUrl,
            ReturnUrl = payment.ReturnUrl,
        };

        PaymentData paymentData = new PaymentData(request.OrderCode, request.Amount, request.Description, request.Items, request.CancelUrl, request.ReturnUrl, null, null, null, null, null, GetExpireAt())!;

        PayOS payOS = new PayOS(_payOSSettings.ClientID, _payOSSettings.ApiKey, _payOSSettings.ChecksumKey)!;

        var createPaymentResult = await payOS.createPaymentLink(paymentData);

        return createPaymentResult.checkoutUrl;
    }

    private static int GetExpireAt()
    {
        // Lấy thời gian hiện tại
        DateTime currentTime = DateTime.UtcNow;

        // Thêm 5 phút vào thời gian hiện tại
        DateTime expirationTime = currentTime.AddMinutes(5);

        // Chuyển đổi thời gian hết hạn thành Unix Timestamp
        int unixTimestamp = (int)(expirationTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        return unixTimestamp;
    }
}
