using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chat.UserService.Migrations
{
    /// <inheritdoc />
    public partial class AddUsernameToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE users ADD COLUMN IF NOT EXISTS \"Username\" character varying(100);"
            );

            migrationBuilder.Sql(
                "UPDATE users SET \"Username\" = lower(\"Id\") WHERE \"Username\" IS NULL OR btrim(\"Username\") = '';"
            );

            migrationBuilder.Sql(
                "ALTER TABLE users ALTER COLUMN \"Username\" SET NOT NULL;"
            );

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_users_Username\" ON users (\"Username\");"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_users_Username\";");
            migrationBuilder.Sql("ALTER TABLE users DROP COLUMN IF EXISTS \"Username\";");
        }
    }
}
