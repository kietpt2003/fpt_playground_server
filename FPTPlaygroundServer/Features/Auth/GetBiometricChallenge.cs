using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FPTPlaygroundServer.Features.Auth;

[ApiController]
[JwtValidationFilter]
[RolesFilter(Role.User)]
public class GetBiometricChallenge : ControllerBase
{
    [HttpGet("auth/biometrics/challenge")]
    [Tags("Auth")]
    [SwaggerOperation(
        Summary = "Get Biometrics Challenge",
        Description = """
        This API is for user get biometrics challenge. 
        """
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler([FromServices] CurrentUserService currentUserService)
    {
        var user = await currentUserService.GetCurrentUser();
        if (user!.Status == UserStatus.Inactive || user.Account.Status != AccountStatus.Active)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("user", "Your account have been inactive or not deactivate")
                .Build();
        }

        var challenge = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        return Ok(new { challenge });
    }
}
