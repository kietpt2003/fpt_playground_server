using FluentValidation;
using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Cryptography;
using System.Text;

namespace FPTPlaygroundServer.Features;

[ApiController]
[JwtValidationFilter]
[RolesFilter(Role.User)]
public class VerifyBiometricSignature : ControllerBase
{
    public new class Request
    {
        public string Challenge { get; set; } = default!;
        public string Signature { get; set; } = default!;
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.Challenge)
                .NotEmpty()
                .WithMessage("Challenge cannot be empty");

            RuleFor(r => r.Signature)
                .NotEmpty()
                .WithMessage("Signature cannot be empty");
        }
    }

    [HttpPost("auth/biometrics/verify")]
    [Tags("Auth")]
    [SwaggerOperation(
        Summary = "Verify Biometrics Challenge",
        Description = """
        This API is for user verify biometrics challenge. 
        """
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler([FromBody] Request request, [FromServices] CurrentUserService currentUserService)
    {
        var user = await currentUserService.GetCurrentUser();
        if (user!.Status == UserStatus.Inactive || user.Account.Status != AccountStatus.Active)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("user", "Your account have been inactive or not deactivate")
                .Build();
        }

        if (user.BiometricPublicKey == null)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("biometrics", "Don't biometrics information")
                .Build();
        }

        using var rsa = new RSACryptoServiceProvider();
        rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(user.BiometricPublicKey), out _);

        var sha256 = SHA256.Create();
        byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(request.Challenge));

        bool isValid = rsa.VerifyHash(hash, Convert.FromBase64String(request.Signature), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        if (isValid)
        {
            return Ok();
        } else
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("biometrics", "Validation failed")
                .Build();
        }
    }
}
