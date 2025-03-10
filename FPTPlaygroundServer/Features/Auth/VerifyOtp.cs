using FluentValidation;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Services.Auth.Models;
using FPTPlaygroundServer.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Data.Entities;
using OtpNet;

namespace FPTPlaygroundServer.Features.Auth;

[ApiController]
[JwtValidationFilter]
[RequestValidation<Request>]
public class VerifyOtp : ControllerBase
{
    public new class Request
    {
        public string Otp { get; set; } = default!;
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.Otp)
                .NotEmpty()
                .WithMessage("Code cannot be empty")
                .Length(6)
                .WithMessage("Otp must be 6 digits");
        }
    }

    [HttpPost("auth/authenticator/verify")]
    [Tags("Auth")]
    [SwaggerOperation(
        Summary = "Verify Google Authenticator",
        Description = """
        This API is for user get secret key and QR code for Google Authenticator. 
        """
    )]
    [ProducesResponseType(typeof(GoogleAuthenticatorResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler([FromServices] CurrentUserService currentUserService, [FromBody] Request request, [FromServices] GoogleAuthenticatorService googleAuthenticatorService)
    {
        var user = await currentUserService.GetCurrentUser();
        if (user!.Status == UserStatus.Inactive || user.Account.Status != AccountStatus.Active)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("user", "Your account have been inactive or not deactivate")
                .Build();
        }

        var totp = new Totp(Base32Encoding.ToBytes(user.AuthenticatorSecretKey));
        bool isValid = totp.VerifyTotp(request.Otp, out _, VerificationWindow.RfcSpecifiedNetworkDelay);

        if (isValid)
        {
            return Ok();
        } else
        {
            return BadRequest();
        }
    }
}
