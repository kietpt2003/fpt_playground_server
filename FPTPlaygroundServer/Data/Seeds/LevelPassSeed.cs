﻿using FPTPlaygroundServer.Data.Entities;

namespace FPTPlaygroundServer.Data.Seeds;

public static class LevelPassSeed
{
    public readonly static List<LevelPass> Default =
     [
        new LevelPass { Id = Guid.Parse("9fa359ec-c84a-452b-a5ec-ccda73b945aa"), Level = 0, Require = 0, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("7c1c6ffd-c28f-4c2a-8d32-5ec170221b99"), CoinValue = 200, Level = 1, Require = 200, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("b52599d9-acf8-4643-94f0-7b06e7ffae83"), CoinValue = 200, Level = 2, Require = 400, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("5c38d8a7-ea83-47cb-bff1-c1a7d88ddbef"), CoinValue = 200, Level = 3, Require = 600, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("a1412c42-b347-4ff5-bc9e-3d81bf49a831"), CoinValue = 200, Level = 4, Require = 800, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("4d8de5b0-b277-4e94-ba6b-df5911aae63e"), CoinValue = 250, DiamondValue = 100, Level = 5, Require = 1000, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("fe497a60-95f1-4c60-b73b-a2a03402f985"), CoinValue = 200, Level = 6, Require = 1200, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("a471d112-2746-4c0b-aa46-7cb8cb3d690e"), CoinValue = 200, Level = 7, Require = 1400, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("004f55e1-6342-4deb-a054-ef5e55122288"), CoinValue = 200, Level = 8, Require = 1600, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("95a2dfc9-9270-4ce7-bf91-b0089514d105"), CoinValue = 200, Level = 9, Require = 1800, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("8c198384-2dd2-4f35-812c-b0ddcd3cb1b6"), CoinValue = 250, DiamondValue = 150, Level = 10, Require = 2000, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("6b34a26b-8099-44f4-bf7b-a232f99385dd"), CoinValue = 200, Level = 11, Require = 2200, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("76cbbfff-312e-43f2-a84a-36262e60f492"), CoinValue = 200, Level = 12, Require = 2400, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("829a5767-b364-4f74-9c2e-406dbf6c9ddc"), CoinValue = 200, Level = 13, Require = 2600, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("f4c65a52-85aa-4eb6-8b64-64efb12bac7d"), CoinValue = 200, Level = 14, Require = 2800, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("7fd99d97-149d-48d2-97f9-d90b14bd7753"), CoinValue = 250, DiamondValue = 100, Level = 15, Require = 3000, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("8fe838ce-bfb2-4ab9-969f-f9927af40cc6"), CoinValue = 200, Level = 16, Require = 3200, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("5bf3e16f-451c-4636-8b29-d76f4c2d256a"), CoinValue = 200, Level = 17, Require = 3400, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("0216c7a5-62e2-4018-b210-1d7735283f40"), CoinValue = 200, Level = 18, Require = 3600, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("1ce16595-f16c-4fb9-b4c2-c791f2c7805c"), CoinValue = 200, Level = 19, Require = 3800, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("a651dc72-65a4-4396-894c-16e7ee086897"), CoinValue = 250, DiamondValue = 150, Level = 20, Require = 4000, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("aaf3b225-8073-46cf-9b71-cc77ad7bf62a"), CoinValue = 200, Level = 21, Require = 4200, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("69f64076-86f2-4f25-9bf0-30de8b60dd84"), CoinValue = 200, Level = 22, Require = 4400, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("dc81ec43-3c57-4fec-b6b0-5837d55f45b3"), CoinValue = 200, Level = 23, Require = 4600, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("2d04ad87-1c51-4654-86c2-e70ee80a307f"), CoinValue = 200, Level = 24, Require = 4800, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("5a94209b-a816-4f11-97f0-a9d782f4b859"), CoinValue = 250, DiamondValue = 100, Level = 25, Require = 5000, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("16969828-1f4f-43e4-99cd-549e9c27a78d"), CoinValue = 200, Level = 26, Require = 5200, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("8fa72979-6031-4b13-8253-fc3571b40f28"), CoinValue = 200, Level = 27, Require = 5400, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("e6b0bd06-be95-441b-88aa-46c5422eb55e"), CoinValue = 200, Level = 28, Require = 5600, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("070c5199-3922-4bea-8469-4e16b74c680a"), CoinValue = 200, Level = 29, Require = 5800, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("2f027aef-edf1-41e5-8dc0-50d87398052e"), CoinValue = 250, DiamondValue = 150, Level = 30, Require = 6000, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("063e7151-87f5-4058-b4e7-1f0d954e0ffe"), CoinValue = 200, Level = 31, Require = 6200, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("d6ef04f3-8b20-4931-9a42-1df0130c6aa6"), CoinValue = 200, Level = 32, Require = 6400, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("5cbc9df4-dee3-4175-90f6-d6a58afb5f66"), CoinValue = 200, Level = 33, Require = 6600, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("1c9d3bf1-d75e-4181-9a2c-b096ec9b6fd1"), CoinValue = 200, Level = 34, Require = 6800, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("bcf738be-82bc-40ad-bd6f-de96e36d70d7"), CoinValue = 250, DiamondValue = 100, Level = 35, Require = 7000, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("c82bd4eb-1f2c-4444-b183-b167f4db13c0"), CoinValue = 200, Level = 36, Require = 7200, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("147a1738-4379-4f8a-a6f1-38b8ac560dad"), CoinValue = 200, Level = 37, Require = 7400, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("e3887c1d-7bf5-4b82-aedd-3ce9d49f3474"), CoinValue = 200, Level = 38, Require = 7600, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("8c26aa0c-72d8-4319-9ed3-9535ed0da9aa"), CoinValue = 200, Level = 39, Require = 7800, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("b35fb890-6a5c-4123-82c9-3460c3b46bf2"), CoinValue = 250, DiamondValue = 150, Level = 40, Require = 8000, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("fcca004e-5a13-4db9-b364-5855f94064c7"), CoinValue = 200, Level = 41, Require = 8200, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("ab6b355a-400a-4f2b-aeb0-58eea2625971"), CoinValue = 200, Level = 42, Require = 8400, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("38907397-8be1-4ce5-82a2-e2fbe740fd40"), CoinValue = 200, Level = 43, Require = 8600, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("5c96dbaf-52fe-4584-92e4-bcfb608eaef7"), CoinValue = 200, Level = 44, Require = 8800, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("a1ce39b3-90d6-42c8-a500-3164d78cb1a4"), CoinValue = 250, DiamondValue = 100, Level = 45, Require = 9000, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("9e06f861-7551-431a-a2b5-5324fd16730c"), CoinValue = 200, Level = 46, Require = 9200, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("00ca485b-01e3-4fe1-a340-19a291b76375"), CoinValue = 200, Level = 47, Require = 9400, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("88871b44-6b4d-48de-b2d9-3de99b59bcf3"), CoinValue = 200, Level = 48, Require = 9600, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("dcfc0fed-f22f-47f6-902e-d34cfd8828ac"), CoinValue = 200, Level = 49, Require = 9800, Status = LevelPassStatus.Active},
        new LevelPass { Id = Guid.Parse("08fe9461-4c6e-4465-bb52-b2cc53242ef0"), CoinValue = 250, DiamondValue = 150, Level = 50, Require = 10000, Status = LevelPassStatus.Active},
     ];
}
