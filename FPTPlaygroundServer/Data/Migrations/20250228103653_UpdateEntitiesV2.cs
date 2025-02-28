using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FPTPlaygroundServer.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntitiesV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Mates",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "Mates",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Mates_UpdatedBy",
                table: "Mates",
                column: "UpdatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Mates_Users_UpdatedBy",
                table: "Mates",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mates_Users_UpdatedBy",
                table: "Mates");

            migrationBuilder.DropIndex(
                name: "IX_Mates_UpdatedBy",
                table: "Mates");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Mates");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Mates");
        }
    }
}
