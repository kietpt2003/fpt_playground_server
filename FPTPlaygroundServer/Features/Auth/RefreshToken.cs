using FluentValidation;
using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Features.Auth.Models;
using FPTPlaygroundServer.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FPTPlaygroundServer.Features.Auth;

[ApiController]
[RequestValidation<Request>]
public class RefreshTokenController : ControllerBase
{
    public new record Request(string RefreshToken);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.RefreshToken)
                .NotEmpty()
                .WithMessage("Token cannot be empty");
        }
    }

    [HttpPost("auth/refresh")]
    [Tags("Auth")]
    [SwaggerOperation(
        Summary = "Refresh Token",
        Description = "This API is for refreshing a new token. Note:" +
                            "<br>&nbsp; - Account bị Inactive thì vẫn gửi refreshToken được (Vì liên quan đến tiền trong ví)."
    )]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler([FromBody] Request request, [FromServices] AppDbContext context, [FromServices] TokenService tokenService)
    {
        var tokenResponse = await tokenService.ValidateRefreshToken(request.RefreshToken, context);

        return Ok(tokenResponse);
    }
}
