using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FPTPlaygroundServer.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserLevelPass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsClaim",
                table: "UserLevelPasses",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsClaim",
                table: "UserLevelPasses");
        }
    }
}
