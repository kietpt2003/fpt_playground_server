using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Features.FaceValues.Models;

namespace FPTPlaygroundServer.Features.FaceValues.Mappers;

public static class FaceValueMapper
{
    public static FaceValueResponse? ToFaceValueResponse(this FaceValue? fv)
    {
        if (fv != null)
        {
            return new FaceValueResponse
            {
                Id = fv.Id,
                CoinValue = fv.CoinValue,
                DiamondValue = fv.DiamondValue,
                VNDValue = fv.VNDValue,
                Quantity = fv.Quantity,
                StartedDate = fv.StartedDate,
                EndedDate = fv.EndedDate,
                CreatedAt = fv.CreatedAt,
                Status = fv.Status,
            };
        }
        return null;
    }
}
