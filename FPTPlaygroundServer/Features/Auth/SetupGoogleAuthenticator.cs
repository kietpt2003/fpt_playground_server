using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Services.Auth;
using FPTPlaygroundServer.Services.Auth.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace FPTPlaygroundServer.Features.Auth;

[ApiController]
[JwtValidationFilter]
public class SetupGoogleAuthenticator : ControllerBase
{
    [HttpPost("auth/authenticator/setup")]
    [Tags("Auth")]
    [SwaggerOperation(
        Summary = "Setup Google Authenticator",
        Description = """
        This API is for user get secret key and QR code for Google Authenticator. 
        """
    )]
    [ProducesResponseType(typeof(GoogleAuthenticatorResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler([FromServices] CurrentUserService currentUserService, AppDbContext context, [FromServices] GoogleAuthenticatorService googleAuthenticatorService)
    {
        var user = await currentUserService.GetCurrentUser();
        if (user!.Status == UserStatus.Inactive || user.Account.Status != AccountStatus.Active)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("user", "Your account have been inactive or not deactivate")
                .Build();
        }

        string secretKey = googleAuthenticatorService.GenerateSecretKey();
        var currentUser = await context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        currentUser!.AuthenticatorSecretKey = secretKey;
        await context.SaveChangesAsync();

        string qrCodeUrl = googleAuthenticatorService.GetQrCodeUrl(user.Account.Email, secretKey);
        return Ok(new GoogleAuthenticatorResponse { QrCodeUrl = qrCodeUrl});
    }
}
