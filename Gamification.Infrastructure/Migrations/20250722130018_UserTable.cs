using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Gamification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_site_visits_sites_site_id",
                table: "user_site_visits");

            migrationBuilder.AlterColumn<int>(
                name: "site_id",
                table: "user_site_visits",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "user_id",
                table: "user_site_visits",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    password = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.user_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_site_visits_user_id",
                table: "user_site_visits",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_site_visits_sites_site_id",
                table: "user_site_visits",
                column: "site_id",
                principalTable: "sites",
                principalColumn: "site_id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_site_visits_users_user_id",
                table: "user_site_visits",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_site_visits_sites_site_id",
                table: "user_site_visits");

            migrationBuilder.DropForeignKey(
                name: "fk_user_site_visits_users_user_id",
                table: "user_site_visits");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropIndex(
                name: "ix_user_site_visits_user_id",
                table: "user_site_visits");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "user_site_visits");

            migrationBuilder.AlterColumn<int>(
                name: "site_id",
                table: "user_site_visits",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_user_site_visits_sites_site_id",
                table: "user_site_visits",
                column: "site_id",
                principalTable: "sites",
                principalColumn: "site_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
