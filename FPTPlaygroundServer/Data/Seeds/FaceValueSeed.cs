using FPTPlaygroundServer.Data.Entities;

namespace FPTPlaygroundServer.Data.Seeds;

public static class FaceValueSeed
{
    public readonly static List<FaceValue> Default =
    [
        new FaceValue { Id = Guid.Parse("4abf381a-00f3-42f8-bd2b-09529274a170"), CoinValue = 10000, DiamondValue = 0, VNDValue = 10000, Quantity = 100, Status = FaceValueStatus.Active, StartedDate = DateTime.UtcNow.Date, CreatedAt = DateTime.UtcNow.Date, UpdatedAt = DateTime.UtcNow.Date },
        new FaceValue { Id = Guid.Parse("1a54d6ae-42df-448d-bbd9-7e9f57f67c23"), CoinValue = 22000, DiamondValue = 0, VNDValue = 20000, Quantity = 80, Status = FaceValueStatus.Active, StartedDate = DateTime.UtcNow.Date, CreatedAt = DateTime.UtcNow.Date, UpdatedAt = DateTime.UtcNow.Date },
        new FaceValue { Id = Guid.Parse("6c0e474b-bd7b-437a-a25e-b872ab885590"), CoinValue = 55000, DiamondValue = 0, VNDValue = 50000, Quantity = 50, Status = FaceValueStatus.Active, StartedDate = DateTime.UtcNow.Date, CreatedAt = DateTime.UtcNow.Date, UpdatedAt = DateTime.UtcNow.Date },
        new FaceValue { Id = Guid.Parse("f3eed832-6fd2-4e2f-a403-e218108919d3"), CoinValue = 120000, DiamondValue = 0, VNDValue = 100000, Quantity = 20, Status = FaceValueStatus.Active, StartedDate = DateTime.UtcNow.Date, CreatedAt = DateTime.UtcNow.Date, UpdatedAt = DateTime.UtcNow.Date },
        new FaceValue { Id = Guid.Parse("6072041b-d5b8-4191-9aac-fcfb4813557a"), CoinValue = 0, DiamondValue = 100, VNDValue = 10000, Quantity = 100, Status = FaceValueStatus.Active, StartedDate = DateTime.UtcNow.Date, CreatedAt = DateTime.UtcNow.Date, UpdatedAt = DateTime.UtcNow.Date },
        new FaceValue { Id = Guid.Parse("bbfb8dcb-fb57-4634-a036-702f6c787b68"), CoinValue = 0, DiamondValue = 220, VNDValue = 20000, Quantity = 80, Status = FaceValueStatus.Active, StartedDate = DateTime.UtcNow.Date, CreatedAt = DateTime.UtcNow.Date, UpdatedAt = DateTime.UtcNow.Date },
        new FaceValue { Id = Guid.Parse("5c9f9248-9c28-47c6-a7a3-67e589e0184f"), CoinValue = 0, DiamondValue = 550, VNDValue = 50000, Quantity = 50, Status = FaceValueStatus.Active, StartedDate = DateTime.UtcNow.Date, CreatedAt = DateTime.UtcNow.Date, UpdatedAt = DateTime.UtcNow.Date },
        new FaceValue { Id = Guid.Parse("875d2aba-6020-4c0f-baca-c0a1370e429d"), CoinValue = 0, DiamondValue = 1200, VNDValue = 100000, Quantity = 20, Status = FaceValueStatus.Active, StartedDate = DateTime.UtcNow.Date, CreatedAt = DateTime.UtcNow.Date, UpdatedAt = DateTime.UtcNow.Date },
     ];
}
