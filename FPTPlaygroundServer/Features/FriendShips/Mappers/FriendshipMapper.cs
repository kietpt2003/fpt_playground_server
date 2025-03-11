using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Features.FriendShips.Models;

namespace FPTPlaygroundServer.Features.FriendShips.Mappers;

public static class FriendshipMapper
{
    public static FriendshipResponse? ToFriendshipResponse(this Friendship? fs, Guid? userId)
    {
        if (fs != null)
        {
            return new FriendshipResponse
            {
                Id = fs.Id,
                Friend = fs.UserId == userId ? fs.Friend.ToFriendResponse()! : fs.User.ToFriendResponse()!,
                Status = fs.Status,
                CreatedAt = fs.CreatedAt,
                UpdatedAt = fs.UpdatedAt,
            };
        }
        return null;
    }

    public static FriendResponse? ToFriendResponse(this User? u)
    {
        if (u != null)
        {
            return new FriendResponse
            {
                Id = u.Id,
                UserName = u.UserName,
                Name = u.Name,
                AvatarUrl = u.AvatarUrl,
                Status = u.Status,
            };
        }
        return null;
    }
}
