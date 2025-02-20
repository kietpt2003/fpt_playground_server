using FluentValidation;
using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.EntityFrameworkCore;

namespace FPTPlaygroundServer.Features.UserLevelPasses;

[ApiController]
[JwtValidationFilter]
[RequestValidation<Request>]
public class ClaimLevelReward : ControllerBase
{
    public new record Request(
        Guid LevelPassId
    );

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.LevelPassId)
                .NotEmpty().WithMessage("LevelPassId cannot be empty");
        }
    }

    [HttpPut("user-level-pass/claim")]
    [Tags("UserLevelPasses")]
    [SwaggerOperation(
        Summary = "For Claim User Level",
        Description = "This API is for user claim reward of level up."
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler([FromBody] Request request, [FromServices] AppDbContext context, [FromServices] CurrentUserService currentUserService)
    {
        var user = await currentUserService.GetCurrentUser();
        if (user!.Status == UserStatus.Inactive || user.Account.Status != AccountStatus.Active)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("user", "Account have been inactive or not deactivate")
                .Build();
        }

        var userLevelPass = await context.UserLevelPasses
            .Include(ulp => ulp.LevelPass)
            .OrderByDescending(ulp => ulp.LevelPass.Level)
            .FirstOrDefaultAsync(ulp => ulp.UserId == user!.Id && ulp.LevelPassId == request.LevelPassId && ulp.LevelPass.Status == LevelPassStatus.Active) ?? throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_00)
                .AddReason("userLevelPass", "userLevelPass empty")
                .Build();

        if (userLevelPass.IsClaim)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_00)
                .AddReason("userLevelPass", "userLevelPass already Claimed")
                .Build();
        }

        if (userLevelPass.LevelPass.Require > userLevelPass.Experience)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_00)
                .AddReason("userLevelPass", "User not enought experiences")
                .Build();
        }

        if (userLevelPass.LevelPass.CoinValue != null)
        {
            user!.CoinWallet.Amount += (int)userLevelPass.LevelPass.CoinValue;
        }

        if (userLevelPass.LevelPass.DiamondValue != null)
        {
            user!.DiamondWallet.Amount += (int)userLevelPass.LevelPass.DiamondValue;

            UserIncome userIncome = new()
            {
                UserId = user!.Id,
                Value = (int)userLevelPass.LevelPass.DiamondValue,
                Type = UserIncomeType.DailyCheckpoint,
                CreatedAt = DateTime.UtcNow,
            };
            await context.UserIncomes.AddAsync(userIncome);
        }

        userLevelPass.IsClaim = true;

        await context.SaveChangesAsync();

        return Ok();
    }
}
