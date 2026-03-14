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
            migrationBuilder.Sql(
                "ALTER TABLE auth_accounts ADD COLUMN IF NOT EXISTS \"Username\" character varying(100) NOT NULL DEFAULT '';"
            );

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_auth_accounts_Username\" ON auth_accounts (\"Username\");"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_auth_accounts_Username\";");
            migrationBuilder.Sql("ALTER TABLE auth_accounts DROP COLUMN IF EXISTS \"Username\";");
        }
    }
}
