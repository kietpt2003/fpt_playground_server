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
public class UpdateUserLevel : ControllerBase
{
    public new record Request(
        int Exp
    );

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.Exp)
                .NotEmpty().WithMessage("Exp cannot be empty")
                .GreaterThan(0)
                .LessThanOrEqualTo(500)
                .WithMessage("Exp from 1 to 500");
        }
    }

    [HttpPut("user-level-pass")]
    [Tags("UserLevelPasses")]
    [SwaggerOperation(
        Summary = "For Update User Level",
        Description = "This API is for user level up."
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
            .FirstOrDefaultAsync(ulp => ulp.UserId == user!.Id && ulp.LevelPass.Require >= ulp.Experience && ulp.LevelPass.Status == LevelPassStatus.Active) ?? throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_00)
                .AddReason("userLevelPass", "userLevelPass empty")
                .Build();

        var finalLevel = await context.LevelPasses
            .OrderByDescending(lp => lp.Level)
            .FirstOrDefaultAsync(lp => lp.Status == LevelPassStatus.Active);

        if (userLevelPass.Experience == userLevelPass.LevelPass.Require && finalLevel!.Level == userLevelPass.LevelPass.Level)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_00)
                .AddReason("userLevelPass", "User maximum level")
                .Build();
        }

        int newExp = request.Exp;
        while (newExp > 0)
        {
            int expRequire = userLevelPass.LevelPass.Require - userLevelPass.Experience;
            if (expRequire >= newExp) //Nếu exp yêu cầu(exp mà level hiện tại còn thiếu) lớn hơn newExp thì tăng cộng thêm vô, else chỉ đắp 1 phần cần thiết thôi và phần sau sẽ vô level tiếp theo
            {
                userLevelPass.Experience += newExp;
                newExp = 0;
            }
            else
            {
                newExp -= expRequire;
                userLevelPass.Experience = userLevelPass.LevelPass.Require;
            }

            await context.SaveChangesAsync();

            if (newExp > 0)
            {
                var nextLevelPass = await context.LevelPasses.FirstOrDefaultAsync(lp => lp.Level == userLevelPass!.LevelPass.Level + 1);

                if (nextLevelPass == null) // Nếu mà null chứng tỏ User đã tới level cuối
                {
                    break;
                }

                UserLevelPass newUserLevelPass = new()
                {
                    UserId = user!.Id,
                    LevelPassId = nextLevelPass.Id,
                    Experience = 0,
                    IsClaim = false,
                };

                await context.UserLevelPasses.AddAsync(newUserLevelPass);
                await context.SaveChangesAsync();

                userLevelPass = newUserLevelPass; //Cập nhật lại userLevelPass để tiếp tục loop
            }
        }

        return Ok();
    }
}
