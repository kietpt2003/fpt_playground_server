using FluentValidation;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Services.Auth;
using FPTPlaygroundServer.Features.Auth.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Cryptography;
using FPTPlaygroundServer.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FPTPlaygroundServer.Features.Auth;

[ApiController]
[RequestValidation<Request>]
public class LoginaccountController : ControllerBase
{
    private const int SaltSize = 16; // 128 bit 
    private const int KeySize = 32;  // 256 bit
    private const int Iterations = 10000; // Number of PBKDF2 iterations

    public new record Request(string Email, string Password, Guid ServerId, string? DeviceToken);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.Email)
                .NotEmpty()
                .WithMessage("Email cannot be empty")
                .EmailAddress()
                .WithMessage("Invalid email");
            RuleFor(r => r.Password)
                .NotEmpty().WithMessage("Password cannot be empty")
                .MinimumLength(8).WithMessage("Password must be between 8 and 15 characters long")
                .MaximumLength(15).WithMessage("Password must be between 8 and 15 characters long")
                .Matches(@"^(?=.*[A-Z])(?=.*\W).{8,}$")
                .WithMessage("Password must contain at least 1 uppercase letter and 1 special character");
        }
    }

    [HttpPost("auth/login")]
    [Tags("Auth")]
    [SwaggerOperation(
        Summary = "Login account",
        Description = "This API is for account login. Note:" +
                            "<br>&nbsp; - deviceToken: Dùng để gửi notification (mỗi 1 máy chỉ có duy nhất 1 deviceToken)." +
                            "<br>&nbsp; - 1 acc thì có thể được đăng nhập bằng nhiều thiết bị (điện thoại, laptop)." +
                            "<br>&nbsp; - deviceToken: Không gửi hoặc để trống cũng được, nhưng tức là thiết bị đó sẽ không nhận được notification thông qua FCM." +
                            "<br>&nbsp; - Hoặc sẽ nhận được notification nếu acc này đã lưu những deviceToken trước đó thì sẽ sẽ gửi noti đến những device đã đk vs acc này." +
                            "<br>&nbsp; - account bị Inactive thì vẫn Login vô được (Vì liên quan đến tiền trong ví)."
    )]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(FPTPlaygroundErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(FPTPlaygroundErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(FPTPlaygroundErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Handler([FromBody] Request request, [FromServices] AppDbContext context, [FromServices] TokenService tokenService)
    {
        var account = await context.Accounts.FirstOrDefaultAsync(a => a.Email == request.Email) ?? throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_00)
                .AddReason("account", "Account not exist")
                .Build();

        if (!VerifyHashedPassword(account.Password!, request.Password))
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_02)
                .AddReason("password", "Incorrect password")
                .Build();
        }

        if (account.Status == AccountStatus.Pending)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("account", "Account not verify")
                .Build();
        }

        if (account.LoginMethod == LoginMethod.Google)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_02)
                .AddReason("account", "This account login by Google")
                .Build();
        }

        var server = await context.Servers.FirstOrDefaultAsync(s => s.Id == request.ServerId) ?? throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_02)
                .AddReason("server", "Server not exist")
                .Build();

        if (!string.IsNullOrEmpty(request.DeviceToken) && !account.Devices.Any(d => d.Token == request.DeviceToken))
        {
            context.Devices.Add(new Device
            {
                AccountId = account.Id,
                Token = request.DeviceToken!,
            });
            await context.SaveChangesAsync();
        }

        var user = await context.Users.FirstOrDefaultAsync(u => u.AccountId == account.Id && u.ServerId == request.ServerId) ?? throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_00)
                .AddReason("user", $"This account doesn't have user on server {request.ServerId}")
                .Build();

        string token = tokenService.CreateToken(user.Id);
        string refreshToken = tokenService.CreateRefreshToken(user.Id);

        return Ok(new TokenResponse
        {
            Token = token,
            RefreshToken = refreshToken
        });
    }

    private static bool VerifyHashedPassword(string hashedPassword, string passwordToCheck)
    {
        var hashBytes = Convert.FromBase64String(hashedPassword);

        var salt = new byte[SaltSize];
        Array.Copy(hashBytes, 0, salt, 0, SaltSize);

        using (var algorithm = new Rfc2898DeriveBytes(passwordToCheck, salt, Iterations, HashAlgorithmName.SHA256))
        {
            var keyToCheck = algorithm.GetBytes(KeySize);
            for (int i = 0; i < KeySize; i++)
            {
                if (hashBytes[i + SaltSize] != keyToCheck[i])
                {
                    return false;
                }
            }
        }

        return true;
    }
}
