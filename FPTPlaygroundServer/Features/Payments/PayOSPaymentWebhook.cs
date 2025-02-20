using FPTPlaygroundServer.Data;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;
using Swashbuckle.AspNetCore.Annotations;
using FPTPlaygroundServer.Features.Payments.Models;
using Microsoft.EntityFrameworkCore;

namespace FPTPlaygroundServer.Features.Payments;

[ApiController]
public class PayOSPaymentWebhook : ControllerBase
{
    [HttpPost("webhook/payos")]
    [Tags("Webhook")]
    [SwaggerOperation(
            Summary = "Payment Transfer Handler - Webhook API - DO NOT USE!!!",
            Description = "API for PayOS transfer data to Project Server" +
                            "<br>&nbsp; - FE do not use this API. This API is used by PayOS to transfer data to Project Server." +
                            "<br>&nbsp; - PayOS will call this API when the payment is successful."
        )]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler(WebhookType body, AppDbContext context)
    {
        try
        {
            var walletTrackingDetail = await context.WalletTrackings
                .Include(wt => wt.FaceValue)
                .FirstOrDefaultAsync(wt => wt.PaymentCode == body.data.orderCode);
            if (walletTrackingDetail != null)
            {
                var userCoinWallet = await context.CoinWallets.FirstOrDefaultAsync(w => w.Id == walletTrackingDetail.CoinWalletId);
                if (userCoinWallet != null)
                {
                    userCoinWallet.Amount += walletTrackingDetail.FaceValue.CoinValue;
                    walletTrackingDetail.DepositedAt = DateTime.UtcNow;
                }

                var userDiamondWallet = await context.DiamondWallets.FirstOrDefaultAsync(w => w.Id == walletTrackingDetail.DiamondWalletId);
                if (userDiamondWallet != null)
                {
                    userDiamondWallet.Amount += walletTrackingDetail.FaceValue.DiamondValue;
                    walletTrackingDetail.DepositedAt = DateTime.UtcNow;
                }

                walletTrackingDetail.Status = Data.Entities.WalletTrackingStatus.Success;
                await context.SaveChangesAsync();
            }
            return Ok(new PaymentResponse(0, "Ok", null));
        }
        catch (Exception)
        {
            return Ok(new PaymentResponse(-1, "fail", null));
        }
    }
}
