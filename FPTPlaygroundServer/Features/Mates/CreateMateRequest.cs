using FluentValidation;
using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Services.Auth;
using FPTPlaygroundServer.Services.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using Swashbuckle.AspNetCore.Annotations;

namespace FPTPlaygroundServer.Features.Mates;

[ApiController]
[JwtValidationFilter]
[RolesFilter(Role.User)]
[RequestValidation<Request>]
public class CreateMateRequest : ControllerBase
{
    public new class Request
    {
        public Guid FriendId { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(c => c.FriendId)
                .NotEmpty()
                .WithMessage("FriendId cannot be empty");
        }
    }

    [HttpPost("mate")]
    [Tags("Mates")]
    [SwaggerOperation(
        Summary = "Send Mate Request",
        Description = """
        This API is for user send mate request. Note: 
        - Dùng api này để gửi yêu cầu hẹn hò
        - Nếu user Undated thì User không thể tái hẹn hò lại trong 1 ngày
        - Và sau 1 ngày thì nếu họn hò lại thì sẽ xóa Mate hiện tại đi và tạo mới lại Mate khác
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
            .FirstOrDefaultAsync(u => u.Id == request.FriendId) ?? throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_00)
                .AddReason("friend", "Friend not found")
                .Build();

        var currentDating = await context.Mates.FirstOrDefaultAsync(m => (m.UserId == user.Id && m.MateId == friend.Id) || (m.MateId == user.Id && m.UserId == friend.Id));
        if (currentDating != null && currentDating.Status == MateStatus.Dated)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_02)
                .AddReason("mate", "You're in a relationship")
                .Build();
        } else if (currentDating != null && currentDating.Status == MateStatus.Pending)
        {
            string errorMsg = currentDating.UpdatedBy == user.Id ? "You're sending a mate request before, please wait or cancel first" : "You have a mate request, please cancel first";
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_02)
                .AddReason("mate-request", errorMsg)
                .Build();
        } else if (currentDating != null && currentDating.Status == MateStatus.Cancelled)
        {
            var remateTime = currentDating.UpdatedAt.AddDays(1);
            var remainingTime = remateTime - currentTime;

            if (remainingTime.TotalSeconds > 0) //Nếu chưa hết thgian cooldown thì báo lỗi
            {
                int remainingHours = (int)remainingTime.TotalHours;
                int remainingMinutes = remainingTime.Minutes;
                int remainingSeconds = remainingTime.Seconds;

                throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_02)
                .AddReason("date", $"Please wait {remainingHours} hours {remainingMinutes} minutes {remainingSeconds} seconds to date again")
                .Build();
            }

            context.Mates.Remove(currentDating);
        }

        if (user.ServerId != friend.ServerId)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("server", "Cannot date outside server")
                .Build();
        }

        bool isYourMateDated = await context.Mates.AnyAsync(m => (m.MateId == friend.Id || m.UserId == friend.Id) && (m.Status == MateStatus.Dated || (m.Status == MateStatus.Cancelled && m.UpdatedAt.AddDays(1) < currentTime)));
        if (isYourMateDated) //TH mate đang Date hoặc đang trong thgian cooldown, còn nếu Pending thì vẫn gửi được bình thường (tức là 1 người sẽ có nhiều Mate request)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("friend", "Your friend are in a relationship")
                .Build();
        }

        if (friend!.Status == UserStatus.Inactive || friend.Account.Status != AccountStatus.Active)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("friend", "Friend have been inactive or not deactivate")
                .Build();
        }

        var currFriendShip = await context.Friendships.FirstOrDefaultAsync(fs => (fs.UserId == user.Id && fs.FriendId == friend.Id) || (fs.UserId == friend.Id && fs.FriendId == user.Id)) ?? throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_00)
                .AddReason("friendship", "Friendship not found")
                .Build();

        if (currFriendShip.Status == FriendshipStatus.Pending)
        {
            string errorMsg = currFriendShip.UserId == user.Id ? "Please wait until your friend accept friend request" : "Please accept friend request before requesting dating";
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_02)
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

        List<string> mateDeviceTokens = friend.Account.Devices.Select(d => d.Token).ToList();
        if (mateDeviceTokens.Count > 0)
        {
            await fcmNotificationService.SendMultibleNotificationAsync(
                mateDeviceTokens,
                "Mate Request",
                $"{user.UserName} send you a mate request",
                new Dictionary<string, string>()
                {
                    { "userId", user.Id.ToString() },
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
                    UserId = user.Id,
                    Title = "Mate Request",
                    Content = $"{user.UserName} send you a mate request",
                    CreatedAt = currentTime,
                    IsRead = false,
                    Type = NotificationType.Mate
                });

                Mate newMate = new()
                {
                    UserId = user.Id,
                    MateId = friend.Id,
                    Status = MateStatus.Pending,
                    CreatedAt = currentTime,
                    UpdatedAt = currentTime,
                    UpdatedBy = user.Id,
                };
                await context.Mates.AddAsync(newMate);
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

        return Created();
    }
}
