using FluentValidation;
using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace FPTPlaygroundServer.Features.DailyCheckpoints;

[ApiController]
[JwtValidationFilter]
[RequestValidation<Request>]
public class UserCheckin : ControllerBase
{
    public new record Request(
        Guid DailyCheckpointId
    );

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.DailyCheckpointId)
                .NotEmpty()
                .WithMessage("DailyCheckpointId cannot be empty");
        }
    }

    [HttpPut("daily-checkpoint")]
    [Tags("DailyCheckpoints")]
    [SwaggerOperation(
        Summary = "For User Checkin",
        Description = "This API is for user checkin."
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler([FromBody] Request request, [FromServices] AppDbContext context, [FromServices] CurrentUserService currentUserService)
    {
        DateTime currentTime = DateTime.UtcNow; //Giờ UTC hiện tại
        DateTime checkInDate = DateTime.UtcNow.Date; // 00:00 AM UTC <=> 07:00 AM VN
        DateTime startCheckin = checkInDate.AddHours(7); // 07:00 AM UTC
        DateTime endOfDay = checkInDate.AddDays(1).AddSeconds(-1); // 23:59:59 PM UT

        var user = await currentUserService.GetCurrentUser();
        if (user!.Status == UserStatus.Inactive || user.Account.Status != AccountStatus.Active)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("user", "Your account have been inactive or not deactivate")
                .Build();
        }

        if (currentTime < startCheckin || currentTime > endOfDay) //Nằm trong khoảng 7h UTC -> 23:59:59 PM UTC
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_02)
                .AddReason("dailyCheckpoint", "Checkin time starts from 7 AM to 11:59 PM (UTC) the same day")
                .Build();
        }

        var userDailyCheckpoint = await context.DailyCheckpoints.FirstOrDefaultAsync(dcp => dcp.Id == request.DailyCheckpointId && dcp.UserId == user!.Id) ?? throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_00)
                .AddReason("dailyCheckpoint", "DailyCheckpoint not exist")
                .Build();

        if (userDailyCheckpoint.CheckInDate.Day != checkInDate.Day)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_02)
                .AddReason("dailyCheckpoint", "This dailyCheckpoint is not for today")
                .Build();
        }

        if (userDailyCheckpoint.Status == DailyCheckpointStatus.Checked)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_02)
                .AddReason("dailyCheckpoint", "User have checked-in today")
                .Build();
        }

        userDailyCheckpoint.Status = DailyCheckpointStatus.Checked;

        user!.CoinWallet.Amount += (int)userDailyCheckpoint.CoinValue!;

        if (userDailyCheckpoint.DiamondValue != null)
        {
            user!.DiamondWallet.Amount += (int)userDailyCheckpoint.DiamondValue!;

            UserIncome userIncome = new()
            {
                UserId = user!.Id,
                Value = (int)userDailyCheckpoint.DiamondValue,
                Type = UserIncomeType.DailyCheckpoint,
                CreatedAt = currentTime,
            };
            await context.UserIncomes.AddAsync(userIncome);
        }
        await context.SaveChangesAsync();

        return Ok();
    }
}
