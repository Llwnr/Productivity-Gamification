using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gamification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NullableSiteVisits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_site_visits_analysis_results_analysis_id",
                table: "user_site_visits");

            migrationBuilder.DropForeignKey(
                name: "fk_user_site_visits_sites_site_id",
                table: "user_site_visits");

            migrationBuilder.AlterColumn<string>(
                name: "site_id",
                table: "user_site_visits",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "analysis_id",
                table: "user_site_visits",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "fk_user_site_visits_analysis_results_analysis_id",
                table: "user_site_visits",
                column: "analysis_id",
                principalTable: "analysis_results",
                principalColumn: "analysis_id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_site_visits_sites_site_id",
                table: "user_site_visits",
                column: "site_id",
                principalTable: "sites",
                principalColumn: "site_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_site_visits_analysis_results_analysis_id",
                table: "user_site_visits");

            migrationBuilder.DropForeignKey(
                name: "fk_user_site_visits_sites_site_id",
                table: "user_site_visits");

            migrationBuilder.AlterColumn<string>(
                name: "site_id",
                table: "user_site_visits",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "analysis_id",
                table: "user_site_visits",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_user_site_visits_analysis_results_analysis_id",
                table: "user_site_visits",
                column: "analysis_id",
                principalTable: "analysis_results",
                principalColumn: "analysis_id",
                onDelete: ReferentialAction.Cascade);

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
