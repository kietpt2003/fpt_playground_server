using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Features.Users.Models;

namespace FPTPlaygroundServer.Features.Users.Mappers;

public static class AccountMapper
{
    public static AccountResponse? ToAccountResponse(this Account? account)
    {
        if (account != null)
        {
            return new AccountResponse
            {
                Id = account.Id,
                Email = account.Email,
                Role = account.Role,
                LoginMethod = account.LoginMethod,
                Status = account.Status,
                CreatedAt = account.CreatedAt
            };
        }
        return null;
    }
}
