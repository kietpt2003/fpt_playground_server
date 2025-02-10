using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FPTPlaygroundServer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNewEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Achievement_Achievement_ParentId",
                table: "Achievement");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAchievements_Achievement_AchievementId",
                table: "UserAchievements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Achievement",
                table: "Achievement");

            migrationBuilder.RenameTable(
                name: "Achievement",
                newName: "Achievements");

            migrationBuilder.RenameIndex(
                name: "IX_Achievement_ParentId",
                table: "Achievements",
                newName: "IX_Achievements_ParentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Achievements",
                table: "Achievements",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "CoinWallets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoinWallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoinWallets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyCheckpoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoinValue = table.Column<int>(type: "integer", nullable: true),
                    DiamondValue = table.Column<int>(type: "integer", nullable: true),
                    CheckInDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyCheckpoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyCheckpoints_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiamondWallets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiamondWallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiamondWallets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FaceValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoinValue = table.Column<int>(type: "integer", nullable: false),
                    DiamondValue = table.Column<int>(type: "integer", nullable: false),
                    VNDValue = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    StartedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaceValues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WalletTrackings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoinWalletId = table.Column<Guid>(type: "uuid", nullable: true),
                    DiamondWalletId = table.Column<Guid>(type: "uuid", nullable: true),
                    FaceValueId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentMethod = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    PaymentCode = table.Column<long>(type: "bigint", nullable: false),
                    DepositedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletTrackings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletTrackings_CoinWallets_CoinWalletId",
                        column: x => x.CoinWalletId,
                        principalTable: "CoinWallets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WalletTrackings_DiamondWallets_DiamondWalletId",
                        column: x => x.DiamondWalletId,
                        principalTable: "DiamondWallets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WalletTrackings_FaceValues_FaceValueId",
                        column: x => x.FaceValueId,
                        principalTable: "FaceValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoinWallets_UserId",
                table: "CoinWallets",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyCheckpoints_UserId",
                table: "DailyCheckpoints",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DiamondWallets_UserId",
                table: "DiamondWallets",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WalletTrackings_CoinWalletId",
                table: "WalletTrackings",
                column: "CoinWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTrackings_DiamondWalletId",
                table: "WalletTrackings",
                column: "DiamondWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTrackings_FaceValueId",
                table: "WalletTrackings",
                column: "FaceValueId");

            migrationBuilder.AddForeignKey(
                name: "FK_Achievements_Achievements_ParentId",
                table: "Achievements",
                column: "ParentId",
                principalTable: "Achievements",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAchievements_Achievements_AchievementId",
                table: "UserAchievements",
                column: "AchievementId",
                principalTable: "Achievements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Achievements_Achievements_ParentId",
                table: "Achievements");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAchievements_Achievements_AchievementId",
                table: "UserAchievements");

            migrationBuilder.DropTable(
                name: "DailyCheckpoints");

            migrationBuilder.DropTable(
                name: "WalletTrackings");

            migrationBuilder.DropTable(
                name: "CoinWallets");

            migrationBuilder.DropTable(
                name: "DiamondWallets");

            migrationBuilder.DropTable(
                name: "FaceValues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Achievements",
                table: "Achievements");

            migrationBuilder.RenameTable(
                name: "Achievements",
                newName: "Achievement");

            migrationBuilder.RenameIndex(
                name: "IX_Achievements_ParentId",
                table: "Achievement",
                newName: "IX_Achievement_ParentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Achievement",
                table: "Achievement",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Achievement_Achievement_ParentId",
                table: "Achievement",
                column: "ParentId",
                principalTable: "Achievement",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAchievements_Achievement_AchievementId",
                table: "UserAchievements",
                column: "AchievementId",
                principalTable: "Achievement",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
