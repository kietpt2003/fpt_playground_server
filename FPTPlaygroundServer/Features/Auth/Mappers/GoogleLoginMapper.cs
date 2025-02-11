using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Features.Auth.Models;

namespace FPTPlaygroundServer.Features.Auth.Mappers;

public static class GoogleLoginMapper
{
    public static Account? ToAccountRequest(this RegisterAccountRequest? registerUserRequest)
    {
        DateTime currentTime = DateTime.UtcNow;
        if (registerUserRequest == null)
        {
            return null;
        }
        return new Account
        {
            Email = registerUserRequest.Email,
            Role = registerUserRequest.Role,
            LoginMethod = registerUserRequest.LoginMethod,
            Status = registerUserRequest.Status,
            CreatedAt = currentTime,
            UpdatedAt = currentTime,
            Devices = registerUserRequest.Devices,
        };
    }
}
