using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FPTPlaygroundServer.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateConversationMessageStatusUserMasked : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConversationIndex",
                table: "Conversations",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConversationIndex",
                table: "Conversations");
        }
    }
}
