﻿using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Features.Servers.Mappers;
using FPTPlaygroundServer.Features.UserLevelPasses.Mappers;
using FPTPlaygroundServer.Features.Users.Models;

namespace FPTPlaygroundServer.Features.Users.Mappers;

public static class UserMapper
{
    public static UserResponse? ToUserResponse(this User? u, UserLevelPass? ulp)
    {
        if (u != null)
        {
            return new UserResponse
            {
                Id = u.Id,
                UserName = u.UserName,
                Name = u.Name,
                AvatarUrl = u.AvatarUrl,
                Gender = u.Gender,
                Grade = u.Grade,
                Status = u.Status,
                LastSeenAt = u.LastSeenAt,
                CreatedAt = u.CreatedAt,
                Account = u.Account.ToAccountResponse()!,
                CoinWallet = u.CoinWallet.ToCoinWalletResponse()!,
                DiamondWallet = u.DiamondWallet.ToDiamondWalletResponse()!,
                Server = u.Server.ToServerResponse(),
                UserLevelPass = ulp.ToUserLevelPassResponse()!,
            };
        }
        return null;
    }
}
