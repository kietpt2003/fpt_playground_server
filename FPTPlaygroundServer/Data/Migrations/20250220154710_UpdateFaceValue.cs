using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FPTPlaygroundServer.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFaceValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalQuantity",
                table: "FaceValues",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalQuantity",
                table: "FaceValues");
        }
    }
}
