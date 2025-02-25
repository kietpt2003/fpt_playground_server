using FluentValidation;
using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using FPTPlaygroundServer.Services.Payment.Models;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using FPTPlaygroundServer.Features.Payments.Models;
using FPTPlaygroundServer.Services.Payment;
using FPTPlaygroundServer.Services.Notifications;
using Net.payOS.Types;

namespace FPTPlaygroundServer.Features.Payments;

[ApiController]
[JwtValidationFilter]
[RolesFilter(Role.User)]
[RequestValidation<Request>]
public class CreatePayment : ControllerBase
{
    public new class Request
    {
        public Guid FaceValueId { get; set; }
        public string? Info { get; set; }
        public WalletTrackingPaymentMethod PaymentMethod { get; set; }
        public string ReturnUrl { get; set; } = default!;
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(sp => sp.FaceValueId)
                .NotEmpty()
                .WithMessage("FaceValueId cannot be empty");

            RuleFor(sp => sp.ReturnUrl)
                .NotEmpty()
                .WithMessage("ReturnUrl không được để trống.");
        }
    }

    [HttpPost("payment")]
    [Tags("Payments")]
    [SwaggerOperation(
        Summary = "Create Payment",
        Description = "This API is for user create payment."
    )]
    [ProducesResponseType(typeof(DepositResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler(
        [FromBody] Request request, AppDbContext context,
        [FromServices] CurrentUserService currentUserService,
        [FromServices] PayOSPaymentSerivce payOSPaymentSerivce,
        [FromServices] FCMNotificationService fcmNotificationService
        )
    {
        var currentTime = DateTime.UtcNow;

        var user = await currentUserService.GetCurrentUser();
        if (user!.Status == UserStatus.Inactive || user.Account.Status != AccountStatus.Active)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("user", "Account have been inactive or not deactivate")
                .Build();
        }

        var faceValue = await context.FaceValues.FirstOrDefaultAsync(fv => fv.Id == request.FaceValueId) ?? throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_00)
                .AddReason("faceValue", "FaceValue not exist")
                .Build();

        if (faceValue.Status != FaceValueStatus.Active)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_02)
                .AddReason("faceValue", "FaceValue has been Inactive")
                .Build();
        }

        if (faceValue.StartedDate > currentTime || currentTime > faceValue.EndedDate)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_02)
                .AddReason("faceValue", "FaceValue out of date")
                .Build();
        }

        if (faceValue.Quantity <= 0)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_02)
                .AddReason("faceValue", "No more redemptions. Please come back tomorrow.")
                .Build();
        }

        var paymentExist = await context.WalletTrackings.FirstOrDefaultAsync(wt => wt.CoinWalletId == user.CoinWallet!.Id && wt.DiamondWalletId == user.DiamondWallet!.Id && WalletTrackingStatus.Pending == wt.Status);
        if (paymentExist != null)
        {
            paymentExist.Status = WalletTrackingStatus.Cancelled;
        }

        long payOSPaymentCode = GenerateDailyRandomLong();
        bool isCodeExist = true;
        int maxNumber = 999999;
        int tryCode = 1;
        do
        {
            isCodeExist = await context.WalletTrackings.AnyAsync(wt => wt.PaymentCode == payOSPaymentCode);
            tryCode++;
        } while (isCodeExist && tryCode <= maxNumber);

        WalletTracking walletTracking = new()
        {
            CoinWalletId = user.CoinWallet.Id,
            DiamondWalletId = user.DiamondWallet.Id,
            FaceValueId = faceValue.Id,
            PaymentMethod = request.PaymentMethod,
            Amount = faceValue.VNDValue,
            Type = WalletTrackingType.Deposit,
            Status = WalletTrackingStatus.Pending,
            PaymentCode = payOSPaymentCode,
        };

        List<ItemData> paymentItems = new List<ItemData>()!;
        if (faceValue.CoinValue != 0 && faceValue.DiamondValue == 0)
        {
            paymentItems.Add(new ItemData($"{faceValue.CoinValue} Coins", 1, faceValue.VNDValue));
        } else if (faceValue.CoinValue == 0 && faceValue.DiamondValue != 0)
        {
            paymentItems.Add(new ItemData($"{faceValue.DiamondValue} Diamonds", 1, faceValue.VNDValue));
        } else
        {
            paymentItems.Add(new ItemData($"{faceValue.CoinValue} Coins + {faceValue.DiamondValue} Diamonds", 1, faceValue.VNDValue));
        }

        PayOSPayment payOSPayment = new()
        {
            PaymentReferenceId = payOSPaymentCode,
            Amount = faceValue.VNDValue,
            Info = request.Info,
            PaymentItems = paymentItems,
            ReturnUrl = request.ReturnUrl,
        }!;

        DepositResponse depositResponse = new()
        {
            DepositUrl = await payOSPaymentSerivce.CreatePaymentAsync(payOSPayment)
        };
        await context.WalletTrackings.AddAsync(walletTracking);
        faceValue.Quantity -= 1;
        await context.SaveChangesAsync();

        try
        {
            List<string> deviceTokens = user!.Account.Devices.Select(d => d.Token).ToList();
            if (deviceTokens.Count > 0)
            {
                await fcmNotificationService.SendMultibleNotificationAsync(
                    deviceTokens,
                    "FPT Playground Payment",
                    "You just requets payment in FPT Playground service Please pay before expired.",
                    new Dictionary<string, string>()
                    {
                        { "walletTrackingId", walletTracking.Id.ToString() },
                    }
                );
            }
            await context.Notifications.AddAsync(new Notification
            {
                UserId = user!.Id,
                Title = "FPT Playground Payment",
                Content = "You just requets payment in FPT Playground service Please pay before expired.",
                CreatedAt = currentTime,
                IsRead = false,
                Type = NotificationType.Deposit
            });
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return Ok(depositResponse);
    }

    private static long GenerateDailyRandomLong()
    {
        // Lấy ngày hiện tại (chỉ lấy phần ngày)
        DateTime today = DateTime.Today;

        // Tạo một seed từ ngày hôm nay bằng cách sử dụng tổng các thành phần ngày
        int seed = today.Year * 10000 + today.Month * 100 + today.Day;

        // Sử dụng seed để tạo Random (mỗi ngày sẽ có seed khác nhau)
        Random dayRandom = new Random(seed);

        // Lấy một số ngẫu nhiên từ seed theo ngày để tạo thành seed cho Random mới
        int randomSeed = dayRandom.Next();

        // Sử dụng DateTime.Now.Ticks để kết hợp thêm tính ngẫu nhiên
        Random random = new Random(randomSeed + (int)(DateTime.Now.Ticks % int.MaxValue));

        // Tạo số ngẫu nhiên từ 100000 đến 999999
        long randomLong = random.Next(100000, 1000000);

        return randomLong;
    }
}
