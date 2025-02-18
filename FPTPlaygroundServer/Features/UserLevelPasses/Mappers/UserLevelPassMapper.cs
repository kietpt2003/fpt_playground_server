using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Features.UserLevelPasses.Models;

namespace FPTPlaygroundServer.Features.UserLevelPasses.Mappers;

public static class UserLevelPassMapper
{
    public static UserLevelPassResponse? ToUserLevelPassResponse(this UserLevelPass? ulp)
    {
        if (ulp != null)
        {
            return new UserLevelPassResponse
            {
                UserId = ulp.UserId,
                LevelPassId = ulp.LevelPassId,
                Level = ulp.LevelPass.Level,
                CoinValue = ulp.LevelPass.CoinValue,
                DiamondValue = ulp.LevelPass.DiamondValue,
                Experience = ulp.Experience,
                Percentage = CalculatePercentage(ulp.Experience, ulp.LevelPass.Require),
                Require = ulp.LevelPass.Require,
                IsClaim = ulp.IsClaim,
            };
        }
        return null;
    }

    public static int CalculatePercentage(int experience, int require)
    {
        if (require == 0) return 0; // Tránh chia cho 0 => trả về hoàn thành 0%
        return (int)Math.Floor((double)experience / require * 100);
    }
}
