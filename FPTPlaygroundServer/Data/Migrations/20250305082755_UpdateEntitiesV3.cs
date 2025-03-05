using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FPTPlaygroundServer.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntitiesV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MaskedDescription",
                table: "MaskedAvatars",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MaskedDescriptionEN",
                table: "MaskedAvatars",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MaskedTitle",
                table: "MaskedAvatars",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaskedDescription",
                table: "MaskedAvatars");

            migrationBuilder.DropColumn(
                name: "MaskedDescriptionEN",
                table: "MaskedAvatars");

            migrationBuilder.DropColumn(
                name: "MaskedTitle",
                table: "MaskedAvatars");
        }
    }
}
