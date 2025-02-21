using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Services.Auth.Models;

namespace FPTPlaygroundServer.Features.Auth.Mappers;

public static class UserMapper
{
    public static TokenRequest? ToTokenRequest(this User? user)
    {
        if (user != null)
        {
            return new TokenRequest
            {
                UserId = user.Id,
                Email = user.Account.Email,
                Role = user.Account.Role
            };
        }
        return null;
    }
}
