using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Features.Users.Models;

namespace FPTPlaygroundServer.Features.Users.Mappers;

public static class CoinWalletMapper
{
    public static CoinWalletResponse? ToCoinWalletResponse(this CoinWallet? cw)
    {
        if (cw != null)
        {
            return new CoinWalletResponse
            {
                Id = cw.Id,
                Amount = cw.Amount,
            };
        }
        return null;
    }
}
