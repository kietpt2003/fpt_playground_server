using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FPTPlaygroundServer.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntitiesV6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "Messages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ParentId",
                table: "Messages",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Messages_ParentId",
                table: "Messages",
                column: "ParentId",
                principalTable: "Messages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Messages_ParentId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ParentId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Messages");
        }
    }
}
