using FluentValidation;
using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Services.VerifyCode;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace FPTPlaygroundServer.Features.Auth;

[ApiController]
[RequestValidation<Request>]
public class SignupAccountController : ControllerBase
{
    public new record Request(string Email, string Password, Role Role, string? DeviceToken);

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
                .Matches(@"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*(),.?""{}|<>_\-])[A-Za-z\d!@#$%^&*(),.?""{}|<>_\-]{8,15}$")
                .WithMessage("Password must contain at least 1 uppercase letter, 1 special character, and 1 digit.");

            RuleFor(r => r.Role)
                .Must(r => r == Role.User)
                .WithMessage("Role must be User");
        }
    }

    [HttpPost("auth/signup")]
    [Tags("Auth")]
    [SwaggerOperation(
        Summary = "Signup Account",
        Description = "This API is for user signup" +
                            "<br>&nbsp; - deviceToken: Dùng để gửi notification (mỗi 1 máy chỉ có duy nhất 1 deviceToken)." +
                            "<br>&nbsp; - 1 acc thì có thể được đăng nhập bằng nhiều thiết bị (điện thoại, laptop)." +
                            "<br>&nbsp; - deviceToken: Không gửi hoặc để trống cũng được, nhưng tức là thiết bị đó sẽ không nhận được notification thông qua FCM." +
                            "<br>&nbsp; - Hoặc sẽ nhận được notification nếu acc này đã lưu những deviceToken trước đó thì sẽ sẽ gửi noti đến những device đã đk vs acc này."
    )]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Handler([FromBody] Request request, [FromServices] AppDbContext context, [FromServices] VerifyCodeService verifyCodeService)
    {
        DateTime currentDate = DateTime.UtcNow;
        var account = new Account
        {
            Email = request.Email,
            Role = request.Role,
            LoginMethod = LoginMethod.Default,
            Status = AccountStatus.Pending,
            Password = HashPassword(request.Password),
            CreatedAt = currentDate,
            UpdatedAt = currentDate,
        };

        if (await context.Accounts.AnyAsync(a => a.Email == account.Email && a.Status == AccountStatus.Pending))
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("account", "Account not verify yet")
                .Build();
        }

        if (await context.Accounts.AnyAsync(us => us.Email == account.Email && us.Status != AccountStatus.Pending))
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_01)
                .AddReason("email", "Email has already been used.")
                .Build();
        }

        if (!string.IsNullOrEmpty(request.DeviceToken))
        {
            account.Devices.Add(new Device
            {
                Token = request.DeviceToken,
            });
        }

        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        await verifyCodeService.SendVerifyCodeAsync(account);

        return Created();
    }

    private static string HashPassword(string password)
    {
        const int SaltSize = 16; // 128 bit 
        const int KeySize = 32;  // 256 bit
        const int Iterations = 10000; // Number of PBKDF2 iterations

        using var algorithm = new Rfc2898DeriveBytes(password, SaltSize, Iterations, HashAlgorithmName.SHA256);
        var salt = algorithm.Salt;
        var key = algorithm.GetBytes(KeySize);

        var hash = new byte[SaltSize + KeySize];
        Array.Copy(salt, 0, hash, 0, SaltSize);
        Array.Copy(key, 0, hash, SaltSize, KeySize);

        return Convert.ToBase64String(hash);
    }
}
