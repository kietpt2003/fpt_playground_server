using FluentValidation;
using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Features.Payments.Models;
using FPTPlaygroundServer.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace FPTPlaygroundServer.Features.Auth;

[ApiController]
[JwtValidationFilter]
[RolesFilter(Role.User)]
[RequestValidation<Request>]
public class UserRegisterBiometric : ControllerBase
{
    public new class Request
    {
        public string PublicKey { get; set; } = default!;
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.PublicKey)
                .NotEmpty()
                .WithMessage("PublicKey cannot be empty");
        }
    }


    [HttpPost("auth/biometrics/register")]
    [Tags("Auth")]
    [SwaggerOperation(
        Summary = "User Register Finger Print",
        Description = """
        This API is for user register finger print. 
        """
    )]
    [ProducesResponseType(typeof(DepositResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler(
        [FromBody] Request request, AppDbContext context,
        [FromServices] CurrentUserService currentUserService
        )
    {
        var currentTime = DateTime.UtcNow;

        var user = await currentUserService.GetCurrentUser();
        if (user!.Status == UserStatus.Inactive || user.Account.Status != AccountStatus.Active)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("user", "Your account have been inactive or not deactivate")
                .Build();
        }

        var updatedUser = await context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        if (updatedUser != null)
        {
            updatedUser.BiometricPublicKey = request.PublicKey;
            updatedUser.UpdatedAt = currentTime;
            await context.SaveChangesAsync();
        }

        return Ok();
    }
}
