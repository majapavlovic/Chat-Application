using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chat.AuthService.Migrations
{
    /// <inheritdoc />
    public partial class AddUsernameToAuthAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "auth_accounts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_auth_accounts_Username",
                table: "auth_accounts",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_auth_accounts_Username",
                table: "auth_accounts");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "auth_accounts");
        }
    }
}
