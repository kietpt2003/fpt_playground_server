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

namespace FPTPlaygroundServer.Features.FriendShips;

[ApiController]
[JwtValidationFilter]
[RolesFilter(Role.User)]
[RequestValidation<Request>]
public class CreateFriendshipRequest : ControllerBase
{
    public new class Request
    {
        public Guid FriendId { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(fs => fs.FriendId)
                .NotEmpty()
                .WithMessage("FriendId cannot be empty");
        }
    }

    [HttpPost("friendship")]
    [Tags("Friendships")]
    [SwaggerOperation(
        Summary = "Send Friend Request",
        Description = """
        This API is for user send friend request. Note: 
        - Dùng api này để gửi lời mời kết bạn
        """
    )]
    [ProducesResponseType(StatusCodes.Status201Created)]
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

        var friend = await context.Users
            .Include(u => u.Account)
            .FirstOrDefaultAsync(u => u.Id == request.FriendId) ?? throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_00)
                .AddReason("user", "User not found")
                .Build();

        if (friend!.Status == UserStatus.Inactive || friend.Account.Status != AccountStatus.Active)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("user", "Friend have been inactive or not deactivate")
                .Build();
        }

        var currFriendShip = await context.Friendships.FirstOrDefaultAsync(fs => fs.UserId == user.Id && fs.FriendId == friend.Id);
        if (currFriendShip is null)
        {
            var currFriendFriendShip = await context.Friendships.FirstOrDefaultAsync(fs => fs.FriendId == user.Id && fs.UserId == friend.Id); //Đảo lại xem coi đối phương đã gửi yêu cầu chưa
            if (currFriendFriendShip is null)
            {
                Friendship newFriendship = new()
                {
                    UserId = user.Id,
                    FriendId = friend.Id,
                    Status = FriendshipStatus.Pending,
                    CreatedAt = currentTime,
                    UpdatedAt = currentTime,
                    UpdatedBy = user.Id,
                };
                await context.Friendships.AddAsync(newFriendship);
                await context.SaveChangesAsync();
            }
            else if (currFriendFriendShip.Status == FriendshipStatus.Pending)
            {
                throw FPTPlaygroundException.NewBuilder()
                    .WithCode(FPTPlaygroundErrorCode.FPB_01)
                    .AddReason("friendship", "Your friend has send request before")
                    .Build();
            }
            else if (currFriendFriendShip.Status == FriendshipStatus.Blocked)
            {
                throw FPTPlaygroundException.NewBuilder()
                    .WithCode(FPTPlaygroundErrorCode.FPB_03)
                    .AddReason("friendship", "Your friend has blocked you")
                    .Build();
            }
            else
            {
                throw FPTPlaygroundException.NewBuilder()
                    .WithCode(FPTPlaygroundErrorCode.FPB_03)
                    .AddReason("friendship", "You cannot update status in this API")
                    .Build();
            }
        }
        else if (currFriendShip.Status == FriendshipStatus.Pending)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_01)
                .AddReason("friendship", "User has send request before")
                .Build();
        }
        else if (currFriendShip.Status == FriendshipStatus.Blocked)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("friendship", "Your friend has been blocked")
                .Build();
        }
        else
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("friendship", "You cannot update status in this API")
                .Build();
        }

        return Created();
    }
}
