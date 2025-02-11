﻿using FluentValidation;
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
    [ProducesResponseType(typeof(FPTPlaygroundErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(FPTPlaygroundErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(FPTPlaygroundErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Handler([FromBody] Request request, [FromServices] AppDbContext context, [FromServices] TokenService tokenService)
    {
        var userInfo = await tokenService.ValidateRefreshToken(request.RefreshToken, context);

        string token = tokenService.CreateToken(userInfo!.Id);
        string refreshToken = tokenService.CreateRefreshToken(userInfo!.Id);

        return Ok(new TokenResponse
        {
            Token = token,
            RefreshToken = refreshToken
        });
    }
}
