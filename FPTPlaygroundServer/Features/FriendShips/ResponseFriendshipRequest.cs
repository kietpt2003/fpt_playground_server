﻿using FluentValidation;
using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Data.Entities;
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
public class ResponseFriendshipRequest : ControllerBase
{
    public new class Request
    {
        public Guid FriendId { get; set; }
        public FriendshipStatus Status { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(c => c.Status)
                .Must(status => status != FriendshipStatus.Pending)
                .WithMessage("Status cannot be Pending");
            RuleFor(c => c.FriendId)
                .NotEmpty()
                .WithMessage("FriendId cannot be empty");
        }
    }

    [HttpPut("friendship")]
    [Tags("Friendships")]
    [SwaggerOperation(
        Summary = "Update Friendship",
        Description = """
        This API is for user change friendship to Accepted, Cancelled, Blocked or Unblocked. Note: 
        - Dùng api này để đồng ý hoặc từ chối hoặc block user, hoặc gỡ chặn user
        - Từ chối kết bạn hoặc gỡ chặn tức là xóa friendship
        - Dùng API này để block luôn cả TH đang là Mate
        """
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
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
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("friend", "Friend not found")
                .Build();

        if (friend!.Status == UserStatus.Inactive || friend.Account.Status != AccountStatus.Active)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("friend", "Your friend account have been inactive or not deactivate")
                .Build();
        }

        Friendship? friendship = new();

        friendship = await context.Friendships
                .Include(fs => fs.User)
                    .ThenInclude(u => u.Account)
                .Include(fs => fs.Friend)
                    .ThenInclude(u => u.Account)
                .FirstOrDefaultAsync(fs => (fs.UserId == user.Id && fs.FriendId == request.FriendId) || (fs.UserId == request.FriendId && fs.FriendId == user.Id));

        if (request.Status != FriendshipStatus.Blocked)
        {
            if (friendship is null)
            {
                throw FPTPlaygroundException.NewBuilder()
                    .WithCode(FPTPlaygroundErrorCode.FPB_03)
                    .AddReason("friendship", "Friendship not found")
                    .Build();
            }

            if (friendship.Status == request.Status)
            {
                throw FPTPlaygroundException.NewBuilder()
                    .WithCode(FPTPlaygroundErrorCode.FPB_03)
                    .AddReason("friendship", "This friendship already in that state")
                    .Build();
            }

            if (request.Status == FriendshipStatus.Accepted && friendship.Status == FriendshipStatus.Pending)
            {
                if (friendship.UserId == user.Id)
                {
                    throw FPTPlaygroundException.NewBuilder()
                    .WithCode(FPTPlaygroundErrorCode.FPV_00)
                    .AddReason("friendship", "You cannot accept your request")
                    .Build();
                }
                try
                {
                    friendship.Status = FriendshipStatus.Accepted;
                    friendship.UpdatedAt = currentTime;
                    friendship.UpdatedBy = user.Id;

                    List<string> userDeviceTokens = friendship.Friend.Account.Devices.Select(d => d.Token).ToList(); //User sẽ là Friend vì người gửi là bạn của User
                    List<string> friendDeviceTokens = friendship.User.Account.Devices.Select(d => d.Token).ToList(); //Bạn của bạn trong lúc này chính là người gửi yêu cầu => họ là User

                    if (friendDeviceTokens.Count > 0)
                    {
                        await fcmNotificationService.SendMultibleNotificationAsync(
                            userDeviceTokens,
                            "Friend Accepted",
                            $"You and {friendship.Friend.UserName} are friends now!",
                            new Dictionary<string, string>()
                            {
                                { "friendShipId", friendship.Id.ToString() },
                            }
                        );
                    }
                    if (userDeviceTokens.Count > 0)
                    {
                        await fcmNotificationService.SendMultibleNotificationAsync(
                            userDeviceTokens,
                            "Friend Accepted",
                            $"You and {friendship.User.UserName} are friends now!",
                            new Dictionary<string, string>()
                            {
                                { "friendShipId", friendship.Id.ToString() },
                            }
                        );
                    }
                    var strategy = context.Database.CreateExecutionStrategy();
                    await strategy.ExecuteAsync(async () =>
                    {
                        using var transaction = await context.Database.BeginTransactionAsync();
                        try
                        {
                            await context.Notifications.AddRangeAsync([new Notification
                            {
                                UserId = friendship.UserId,
                                Title = "Friend Accepted",
                                Content = $"You and {friendship.Friend.UserName} are friends now!",
                                CreatedAt = currentTime,
                                IsRead = false,
                                Type = NotificationType.Friendship
                            },
                             new Notification
                            {
                                UserId = friendship.FriendId,
                                Title = "Friend Accepted",
                                Content = $"You and {friendship.User.UserName} are friends now!",
                                CreatedAt = currentTime,
                                IsRead = false,
                                Type = NotificationType.Friendship
                            }]);
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
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else if (request.Status == FriendshipStatus.Cancelled && (friendship.Status == FriendshipStatus.Pending || friendship.Status == FriendshipStatus.Accepted))
            {
                context.Friendships.Remove(friendship);
                await context.SaveChangesAsync();
            }
            else if (request.Status == FriendshipStatus.Unblocked && friendship.Status == FriendshipStatus.Blocked)
            {
                if (friendship.UserId != user.Id)
                {
                    throw FPTPlaygroundException.NewBuilder()
                    .WithCode(FPTPlaygroundErrorCode.FPV_00)
                    .AddReason("friendship", "You cannot unblocked")
                    .Build();
                }
                context.Friendships.Remove(friendship);
                await context.SaveChangesAsync();
            }
        }
        else
        {
            if (friendship is null) //TH null tức là 2 người này chưa kết bạn mà muốn block luôn
            {
                Friendship newFriendship = new()
                {
                    UserId = user.Id,
                    FriendId = request.FriendId,
                    Status = FriendshipStatus.Blocked,
                    CreatedAt = currentTime,
                    UpdatedAt = currentTime,
                    UpdatedBy = user.Id
                };
                await context.Friendships.AddAsync(newFriendship);
                await context.SaveChangesAsync();
            }
            else //TH 2 người này đã kb
            {
                if (friendship.Status == FriendshipStatus.Blocked)
                {
                    throw FPTPlaygroundException.NewBuilder()
                    .WithCode(FPTPlaygroundErrorCode.FPB_03)
                    .AddReason("friendship", friendship.UpdatedBy == user.Id ? "This friendship already in that state" : "User have been blocked")
                    .Build();
                }
                else
                {
                    friendship.Status = FriendshipStatus.Blocked;
                    friendship.UpdatedAt = DateTime.UtcNow;
                    friendship.UpdatedBy = user.Id;

                    //Check xem coi có phải Mate không, nếu phải thì xóa Mate luôn
                    var currentDating = await context.Mates.FirstOrDefaultAsync(m => (m.UserId == user.Id && m.MateId == friend.Id) || (m.MateId == user.Id && m.UserId == friend.Id));
                    if (currentDating != null)
                    {
                        context.Mates.Remove(currentDating);
                    }

                    await context.SaveChangesAsync();
                }
            }
        }

        return Ok();
    }
}
