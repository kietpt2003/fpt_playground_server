using FluentValidation;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Services.Auth;
using FPTPlaygroundServer.Services.VerifyCode;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using FPTPlaygroundServer.Features.Auth.Models;
using Microsoft.EntityFrameworkCore;
using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Services.Auth.Models;

namespace FPTPlaygroundServer.Features.Auth;

[ApiController]
[RequestValidation<Request>]
public class VerifyAccountController: ControllerBase
{
    public new record Request(string Email, string Code, string? DeviceToken);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.Email)
                .NotEmpty()
                .WithMessage("Email cannot be empty")
                .EmailAddress()
                .WithMessage("Invalid email");

            RuleFor(r => r.Code)
                .NotEmpty()
                .WithMessage("Verify code cannot be empty");
        }
    }

    [HttpPost("auth/verify")]
    [Tags("Auth")]
    [SwaggerOperation(
        Summary = "Verify Account",
        Description = "This API is for verifying a account. Note:" +
                            "<br>&nbsp; - deviceToken: Dùng để gửi notification (mỗi 1 máy chỉ có duy nhất 1 deviceToken)." +
                            "<br>&nbsp; - 1 acc thì có thể được đăng nhập bằng nhiều thiết bị (điện thoại, laptop)." +
                            "<br>&nbsp; - deviceToken: Không gửi hoặc để trống cũng được, nhưng tức là thiết bị đó sẽ không nhận được notification thông qua FCM." +
                            "<br>&nbsp; - Hoặc sẽ nhận được notification nếu acc này đã lưu những deviceToken trước đó thì sẽ sẽ gửi noti đến những device đã đk vs acc này." +
                            "<br>&nbsp; - Account bị Inactive thì vẫn Verify được (Vì liên quan đến tiền trong ví)."
    )]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler([FromBody] Request request,
        [FromServices] AppDbContext context, [FromServices] VerifyCodeService verifyCodeService, [FromServices] TokenService tokenService)
    {
        var account = await context.Accounts
            .Include(a => a.Devices)
            .FirstOrDefaultAsync(a => a.Email == request.Email) ?? throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_00)
                .AddReason("account", "Account not exist")
                .Build();

        await verifyCodeService.VerifyUserAsync(account, request.Code);

        if (!string.IsNullOrEmpty(request.DeviceToken) && !account.Devices.Any(d => d.Token == request.DeviceToken))
        {
            context.Devices.Add(new Device
            {
                AccountId = account.Id,
                Token = request.DeviceToken!,
            });
            await context.SaveChangesAsync();
        }

        TokenRequest tokenRequest = new() { 
            Email = request.Email,
            Role = Role.User
        };
        string token = tokenService.CreateToken(tokenRequest);
        string refreshToken = tokenService.CreateRefreshToken(tokenRequest);

        return Ok(new TokenResponse
        {
            Token = token,
            RefreshToken = refreshToken
        });
    }
}
