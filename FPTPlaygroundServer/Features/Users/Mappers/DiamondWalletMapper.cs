using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Features.Users.Models;

namespace FPTPlaygroundServer.Features.Users.Mappers;

public static class DiamondWalletMapper
{
    public static DiamondWalletResponse? ToDiamondWalletResponse(this DiamondWallet? dw)
    {
        if (dw != null)
        {
            return new DiamondWalletResponse
            {
                Id = dw.Id,
                Amount = dw.Amount,
            };
        }
        return null;
    }
}
