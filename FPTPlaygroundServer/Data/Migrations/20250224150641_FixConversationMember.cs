using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FPTPlaygroundServer.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixConversationMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UPdatedAt",
                table: "ConversationMembers",
                newName: "UpdatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "ConversationMembers",
                newName: "UPdatedAt");
        }
    }
}
