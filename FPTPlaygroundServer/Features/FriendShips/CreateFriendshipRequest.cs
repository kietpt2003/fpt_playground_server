using FluentValidation;
using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Features.Payments.Models;
using FPTPlaygroundServer.Services.Auth;
using FPTPlaygroundServer.Services.Notifications;
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
        [FromServices] CurrentUserService currentUserService,
        [FromServices] FCMNotificationService fcmNotificationService
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
                .ThenInclude(a => a.Devices)
            .FirstOrDefaultAsync(u => u.Id == request.FriendId) ?? throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_00)
                .AddReason("friend", "Friend not found")
                .Build();

        if (user.ServerId != friend.ServerId)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("server", "Cannot make friend outside server")
                .Build();
        }

        if (friend!.Status == UserStatus.Inactive || friend.Account.Status != AccountStatus.Active)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("friend", "Friend have been inactive or not deactivate")
                .Build();
        }

        var currFriendShip = await context.Friendships.FirstOrDefaultAsync(fs => (fs.UserId == user.Id && fs.FriendId == friend.Id) || (fs.FriendId == user.Id && fs.UserId == friend.Id));
        if (currFriendShip is null)
        {
            List<string> friendDeviceTokens = friend.Account.Devices.Select(d => d.Token).ToList();
            if (friendDeviceTokens.Count > 0)
            {
                await fcmNotificationService.SendMultibleNotificationAsync(
                    friendDeviceTokens,
                    "Friend Request",
                    $"{user.UserName} send you a friend request",
                    new Dictionary<string, string>()
                    {
                        { "userId", friend.Id.ToString() },
                    }
                );
            }

            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    await context.Notifications.AddAsync(new Notification
                    {
                        UserId = friend.Id,
                        Title = "Friend Request",
                        Content = $"{user.UserName} send you a friend request",
                        CreatedAt = currentTime,
                        IsRead = false,
                        Type = NotificationType.Friendship
                    });

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

                    // Commit transaction
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    // Rollback nếu có lỗi
                    await transaction.RollbackAsync();
                    throw FPTPlaygroundException.NewBuilder()
                        .WithCode(FPTPlaygroundErrorCode.FPS_00)
                        .AddReason("server", "Something wrong with the server")
                        .Build();
                }
            });
        }
        else if (currFriendShip.Status == FriendshipStatus.Pending)
        {
            string errorMsg = currFriendShip.UserId == user.Id ? "User has send request before" : "Your friend has send request before";
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_01)
                .AddReason("friendship", errorMsg)
                .Build();
        }
        else if (currFriendShip.Status == FriendshipStatus.Blocked)
        {
            string errorMsg = currFriendShip.UpdatedBy == user.Id ? "You has blocked your friend" : "Your friend has blocked you";
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("friendship", errorMsg)
                .Build();
        }
        else
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("friendship", "Bad request")
                .Build();
        }

        return Created();
    }
}
