using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Gamification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SiteVisitTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_analysis_results_site_id",
                table: "analysis_results");

            migrationBuilder.CreateTable(
                name: "user_site_visits",
                columns: table => new
                {
                    site_visit_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    site_id = table.Column<int>(type: "integer", nullable: false),
                    analysis_id = table.Column<int>(type: "integer", nullable: false),
                    visit_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_site_visits", x => x.site_visit_id);
                    table.ForeignKey(
                        name: "fk_user_site_visits_analysis_results_analysis_id",
                        column: x => x.analysis_id,
                        principalTable: "analysis_results",
                        principalColumn: "analysis_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_site_visits_sites_site_id",
                        column: x => x.site_id,
                        principalTable: "sites",
                        principalColumn: "site_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_analysis_results_site_id_user_goal",
                table: "analysis_results",
                columns: new[] { "site_id", "user_goal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_site_visits_analysis_id",
                table: "user_site_visits",
                column: "analysis_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_site_visits_processed_at",
                table: "user_site_visits",
                column: "processed_at",
                filter: "\"processed_at\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_user_site_visits_site_id",
                table: "user_site_visits",
                column: "site_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_site_visits_visit_date",
                table: "user_site_visits",
                column: "visit_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_site_visits");

            migrationBuilder.DropIndex(
                name: "ix_analysis_results_site_id_user_goal",
                table: "analysis_results");

            migrationBuilder.CreateIndex(
                name: "ix_analysis_results_site_id",
                table: "analysis_results",
                column: "site_id");
        }
    }
}
