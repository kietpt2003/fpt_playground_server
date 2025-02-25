using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FPTPlaygroundServer.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMessageStatusEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageStatuses_Users_SenderId",
                table: "MessageStatuses");

            migrationBuilder.RenameColumn(
                name: "SenderId",
                table: "MessageStatuses",
                newName: "ReceiverId");

            migrationBuilder.RenameIndex(
                name: "IX_MessageStatuses_SenderId",
                table: "MessageStatuses",
                newName: "IX_MessageStatuses_ReceiverId");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageStatuses_Users_ReceiverId",
                table: "MessageStatuses",
                column: "ReceiverId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageStatuses_Users_ReceiverId",
                table: "MessageStatuses");

            migrationBuilder.RenameColumn(
                name: "ReceiverId",
                table: "MessageStatuses",
                newName: "SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_MessageStatuses_ReceiverId",
                table: "MessageStatuses",
                newName: "IX_MessageStatuses_SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageStatuses_Users_SenderId",
                table: "MessageStatuses",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
