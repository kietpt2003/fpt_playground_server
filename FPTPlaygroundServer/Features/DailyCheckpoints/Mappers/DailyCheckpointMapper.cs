using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Features.DailyCheckpoints.Models;

namespace FPTPlaygroundServer.Features.DailyCheckpoints.Mappers;

public static class DailyCheckpointMapper
{
    public static DailyCheckPointResponse? ToDailyCheckpointResponse(this DailyCheckpoint? dc)
    {
        if (dc != null)
        {
            return new DailyCheckPointResponse
            {
                Id = dc.Id,
                UserId = dc.UserId,
                CoinValue = dc.CoinValue,
                DiamondValue = dc.DiamondValue,
                CheckInDate = dc.CheckInDate,
                Status = dc.Status,
            };
        }
        return null;
    }
}
