using FluentValidation;
using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Services.VerifyCode;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Cryptography;

namespace FPTPlaygroundServer.Features.Auth;

[ApiController]
[RequestValidation<Request>]
public class ForgotAccountPassword : ControllerBase
{
    private const int SaltSize = 16; // 128 bit 
    private const int KeySize = 32;  // 256 bit
    private const int Iterations = 10000; // Number of PBKDF2 iterations
    public new record Request(
        string Email,
        string NewPassword,
        string Code
    );

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.Email)
                .NotEmpty()
                .WithMessage("Email cannot be empty")
                .EmailAddress()
                .WithMessage("Invalid email");

            RuleFor(r => r.NewPassword)
                .NotEmpty().WithMessage("New password cannot be empty")
                .MinimumLength(8).WithMessage("New password must be between 8 and 15 characters long")
                .MaximumLength(15).WithMessage("New password must be between 8 and 15 characters long")
                .Matches(@"^(?=.*[A-Z])(?=.*\W)(?=.*\d).{8,15}$")
                .WithMessage("New password must contain at least 1 uppercase letter, 1 special character, and 1 digit.");

            RuleFor(r => r.Code)
                .NotEmpty()
                .WithMessage("Verify code cannot be empty");
        }
    }

    [HttpPost("auth/forgot-password")]
    [Tags("Auth")]
    [SwaggerOperation(
        Summary = "Forgot Account Password",
        Description = "This API is for accounts that forgot password to change their password. Note:" +
                            "<br>&nbsp; - Account bị Inactive thì vẫn forgot password được (Vì liên quan đến tiền trong ví)."
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(FPTPlaygroundErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(FPTPlaygroundErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(FPTPlaygroundErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Handler([FromBody] Request request, [FromServices] AppDbContext context, [FromServices] VerifyCodeService verifyCodeService)
    {
        var account = await context.Accounts.FirstOrDefaultAsync(user => user.Email == request.Email) ?? throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_00)
                .AddReason("account", "Account not exist")
                .Build();

        await verifyCodeService.VerifyUserChangePasswordAsync(account, request.Code);
        account.Password = HashPassword(request.NewPassword);
        await context.SaveChangesAsync();

        return Ok("Update password success!");
    }

    private static string HashPassword(string password)
    {
        using var algorithm = new Rfc2898DeriveBytes(password, SaltSize, Iterations, HashAlgorithmName.SHA256);
        var salt = algorithm.Salt;
        var key = algorithm.GetBytes(KeySize);

        var hash = new byte[SaltSize + KeySize];
        Array.Copy(salt, 0, hash, 0, SaltSize);
        Array.Copy(key, 0, hash, SaltSize, KeySize);

        return Convert.ToBase64String(hash);
    }
}
