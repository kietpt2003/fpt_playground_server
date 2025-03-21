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

namespace FPTPlaygroundServer.Features.Mates;

[ApiController]
[JwtValidationFilter]
[RolesFilter(Role.User)]
[RequestValidation<Request>]
public class ResponseMateRequest : ControllerBase
{
    public new class Request
    {
        public Guid FriendId { get; set; }
        public MateStatus Status { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(c => c.Status)
                .Must(status => status != MateStatus.Pending)
                .WithMessage("Status cannot be Pending");
            RuleFor(c => c.FriendId)
                .NotEmpty()
                .WithMessage("MateId cannot be empty");
        }
    }

    [HttpPut("mate")]
    [Tags("Mates")]
    [SwaggerOperation(
        Summary = "Update Mate",
        Description = """
        This API is for user change mate to Dated, Cancelled. Note: 
        - Dùng api này để đồng ý hoặc từ chối(hủy) hẹn hò
        - Từ chối hẹn hò (Tức là từ pending => cancelled) tức là xóa mate => Remove
        - Hủy hẹn hò (Tức là từ dated => cancelled) tức update mate, nhầm tạo cooldown trước khi xóa => Update
        - API này không sử dụng để Block or Unblocked User. Dùng API friendship để làm điều đó
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

        if (user.ServerId != friend.ServerId)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("server", "Cannot date outside server")
                .Build();
        }

        if (friend!.Status == UserStatus.Inactive || friend.Account.Status != AccountStatus.Active)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("mate", "Your mate account have been inactive or not deactivate")
                .Build();
        }

        var friendship = await context.Friendships
                .Include(fs => fs.User)
                    .ThenInclude(u => u.Account)
                .Include(fs => fs.Friend)
                    .ThenInclude(u => u.Account)
                .FirstOrDefaultAsync(fs => (fs.UserId == user.Id && fs.FriendId == friend.Id) || (fs.UserId == friend.Id && fs.FriendId == user.Id)) ?? throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("mate", "You and this person don't have any relationship")
                .Build();

        if (friendship.Status == FriendshipStatus.Blocked)
        {
            throw FPTPlaygroundException.NewBuilder()
            .WithCode(FPTPlaygroundErrorCode.FPB_03)
            .AddReason("friendship", friendship.UpdatedBy == user.Id ? "You cannot do anything while blocking this person" : "User have been blocked")
            .Build();
        }

        var currentDating = await context.Mates.FirstOrDefaultAsync(m => (m.UserId == user.Id && m.MateId == friend.Id) || (m.MateId == user.Id && m.UserId == friend.Id)) ?? throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_00)
                .AddReason("mate-request", "Mate request not found")
                .Build();

        if (currentDating.Status == request.Status)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("mate", "This relationship already in that state")
                .Build();
        }

        var theirConversation = await context.Conversations
                .Include(c => c.ConversationMembers)
                    .ThenInclude(cm => cm.UserMasked)
                .Where(c => c.Type == ConversationType.Personal || c.Type == ConversationType.Dating || c.Type == ConversationType.Friendship)
                .Where(c => c.ConversationMembers.Any(cm => cm.UserId == friendship.FriendId || (cm.UserMasked != null && cm.UserMasked.UserId == friendship.FriendId)) &&
                    c.ConversationMembers.Any(cm => cm.UserId == friendship.UserId || (cm.UserMasked != null && cm.UserMasked.UserId == friendship.UserId))
                ).FirstOrDefaultAsync();

        List<string> userDeviceTokens = currentDating.YourMate.Account.Devices.Select(d => d.Token).ToList(); //User sẽ là YourMate vì người đồng ý phải là User
        List<string> mateDeviceTokens = currentDating.User.Account.Devices.Select(d => d.Token).ToList(); //Mate của bạn trong lúc này chính là người gửi yêu cầu => họ là User
        
        if (request.Status == MateStatus.Dated && currentDating.Status == MateStatus.Pending)
        {
            if (currentDating.UserId == user.Id)
            {
                throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPV_00)
                .AddReason("date-request", "You cannot accept your request")
                .Build();
            }

            try
            {
                currentDating.Status = request.Status;
                currentDating.UpdatedAt = currentTime;
                currentDating.UpdatedBy = user.Id;

                if (mateDeviceTokens.Count > 0)
                {
                    await fcmNotificationService.SendMultibleNotificationAsync(
                        userDeviceTokens,
                        "Mate Accepted",
                        $"You and {currentDating.YourMate.UserName} are in relationship now!",
                        new Dictionary<string, string>()
                        {
                            { "mateTableId", currentDating.Id.ToString() },
                        }
                    );
                }
                if (userDeviceTokens.Count > 0)
                {
                    await fcmNotificationService.SendMultibleNotificationAsync(
                        userDeviceTokens,
                        "Mate Accepted",
                        $"You and {currentDating.User.UserName} are in relationship now!",
                        new Dictionary<string, string>()
                        {
                            { "mateTableId", currentDating.Id.ToString() },
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
                                UserId = currentDating.UserId,
                                Title = "Mate Accepted",
                                Content = $"You and {currentDating.YourMate.UserName} are in relationship now!",
                                CreatedAt = currentTime,
                                IsRead = false,
                                Type = NotificationType.Mate
                            },
                             new Notification
                            {
                                UserId = currentDating.MateId,
                                Title = "Mate Accepted",
                                Content = $"You and {currentDating.User.UserName} are friends now!",
                                CreatedAt = currentTime,
                                IsRead = false,
                                Type = NotificationType.Mate
                            }]);

                        if (theirConversation != null) //Nếu có conversation rồi thì cập nhật lại Type thôi
                        {
                            theirConversation.Name = "Dating Conversation";
                            theirConversation.Type = ConversationType.Dating;
                        }
                        else //Nếu chưa có conversation thì tạo mới
                        {
                            Conversation conversation = new()
                            {
                                Name = "Dating Conversation",
                                Type = ConversationType.Friendship,
                                Status = ConversationStatus.Active,
                                CreatedAt = currentTime,
                                UpdatedAt = currentTime,
                            };

                            ConversationMember currentSender = new()
                            {
                                Conversation = conversation,
                                UserId = friendship.UserId,
                                Role = ConversationMemberRole.Owner,
                                Status = ConversationMemberStatus.Joined,
                                JoinedAt = currentTime,
                                UpdatedAt = currentTime,
                            };

                            ConversationMember currentReceiver = new()
                            {
                                Conversation = conversation,
                                UserId = friendship.FriendId,
                                Role = ConversationMemberRole.Member,
                                Status = ConversationMemberStatus.Joined,
                                JoinedAt = currentTime.AddSeconds(1),
                                UpdatedAt = currentTime.AddSeconds(1),
                            };

                            var strategy = context.Database.CreateExecutionStrategy();
                            await strategy.ExecuteAsync(async () =>
                            {
                                using var transaction = await context.Database.BeginTransactionAsync();
                                try
                                {
                                    await context.ConversationMembers.AddRangeAsync([currentSender, currentReceiver]);

                                    // Lưu tất cả vào database
                                    await context.SaveChangesAsync();

                                    // Commit transaction
                                    await transaction.CommitAsync();
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.ToString());
                                    // Rollback nếu có lỗi
                                    await transaction.RollbackAsync();
                                    if (ex is FPTPlaygroundException fptPlagroundException)
                                    {
                                        throw FPTPlaygroundException.NewBuilder()
                                        .WithCode(fptPlagroundException.ErrorCode)
                                        .AddReasons(fptPlagroundException.GetReasons().Select(reason => new FPTPlaygroundException.Reason(reason.Title, reason.ReasonMessage)))
                                        .Build();
                                    }
                                    else
                                    {
                                        throw FPTPlaygroundException.NewBuilder()
                                        .WithCode(FPTPlaygroundErrorCode.FPS_00)
                                        .AddReason("server", "Something wrong with the server")
                                        .Build();
                                    }
                                }
                            });
                        }

                        await context.SaveChangesAsync();

                        // Commit transaction
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        // Rollback nếu có lỗi
                        await transaction.RollbackAsync();
                        if (ex is FPTPlaygroundException fptPlagroundException)
                        {
                            throw FPTPlaygroundException.NewBuilder()
                            .WithCode(fptPlagroundException.ErrorCode)
                            .AddReasons(fptPlagroundException.GetReasons().Select(reason => new FPTPlaygroundException.Reason(reason.Title, reason.ReasonMessage)))
                            .Build();
                        }
                        else
                        {
                            throw FPTPlaygroundException.NewBuilder()
                            .WithCode(FPTPlaygroundErrorCode.FPS_00)
                            .AddReason("server", "Something wrong with the server")
                            .Build();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        else if (request.Status == MateStatus.Cancelled && currentDating.Status == MateStatus.Pending) //TH Từ chối => Delete
        {
            if (mateDeviceTokens.Count > 0)
            {
                await fcmNotificationService.SendMultibleNotificationAsync(
                    userDeviceTokens,
                    "Mate Not Accepted",
                    $"{currentDating.YourMate.UserName} not accept your mate request",
                    []
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
                        UserId = currentDating.UserId,
                        Title = "Mate Not Accepted",
                        Content = $"{currentDating.YourMate.UserName} not accept your mate request",
                        CreatedAt = currentTime,
                        IsRead = false,
                        Type = NotificationType.Mate
                    });

                    context.Mates.Remove(currentDating);

                    await context.SaveChangesAsync();

                    // Commit transaction
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    // Rollback nếu có lỗi
                    await transaction.RollbackAsync();
                    if (ex is FPTPlaygroundException fptPlagroundException)
                    {
                        throw FPTPlaygroundException.NewBuilder()
                        .WithCode(fptPlagroundException.ErrorCode)
                        .AddReasons(fptPlagroundException.GetReasons().Select(reason => new FPTPlaygroundException.Reason(reason.Title, reason.ReasonMessage)))
                        .Build();
                    }
                    else
                    {
                        throw FPTPlaygroundException.NewBuilder()
                        .WithCode(FPTPlaygroundErrorCode.FPS_00)
                        .AddReason("server", "Something wrong with the server")
                        .Build();
                    }
                }
            });
        } else if (request.Status == MateStatus.Cancelled && currentDating.Status == MateStatus.Dated) //TH hết yêu => Update status
        {
            if (mateDeviceTokens.Count > 0)
            {
                await fcmNotificationService.SendMultibleNotificationAsync(
                    userDeviceTokens,
                    "Unmate Notice",
                    $"{currentDating.YourMate.UserName} undate with you",
                    []
                );
            }

            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    currentDating.Status = MateStatus.Cancelled;
                    currentDating.UpdatedAt = currentTime;
                    currentDating.UpdatedBy = user.Id;

                    await context.Notifications.AddAsync(new Notification
                    {
                        UserId = currentDating.UserId,
                        Title = "Unmate Notice",
                        Content = $"{currentDating.YourMate.UserName} undate with you",
                        CreatedAt = currentTime,
                        IsRead = false,
                        Type = NotificationType.Mate
                    });

                    if (theirConversation != null)
                    {
                        theirConversation.Name = "Friendship Conversation";
                        theirConversation.Type = ConversationType.Friendship;
                    }

                    await context.SaveChangesAsync();

                    // Commit transaction
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    // Rollback nếu có lỗi
                    await transaction.RollbackAsync();
                    if (ex is FPTPlaygroundException fptPlagroundException)
                    {
                        throw FPTPlaygroundException.NewBuilder()
                        .WithCode(fptPlagroundException.ErrorCode)
                        .AddReasons(fptPlagroundException.GetReasons().Select(reason => new FPTPlaygroundException.Reason(reason.Title, reason.ReasonMessage)))
                        .Build();
                    }
                    else
                    {
                        throw FPTPlaygroundException.NewBuilder()
                        .WithCode(FPTPlaygroundErrorCode.FPS_00)
                        .AddReason("server", "Something wrong with the server")
                        .Build();
                    }
                }
            });
        } else
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_02)
                .AddReason("user", "Bad request")
                .Build();
        }

        return Ok();
    }
}
