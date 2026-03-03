using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chat.MessagingService.Migrations
{
    /// <inheritdoc />
    public partial class AddClientMessageId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientMessageId",
                table: "messages",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_messages_ClientMessageId",
                table: "messages",
                column: "ClientMessageId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_messages_ClientMessageId",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "ClientMessageId",
                table: "messages");
        }
    }
}
