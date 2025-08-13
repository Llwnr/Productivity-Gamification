using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gamification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BasicSchemas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sites",
                columns: table => new
                {
                    site_id = table.Column<string>(type: "text", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    url = table.Column<string>(type: "text", nullable: true),
                    title = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sites", x => x.site_id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    username = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    password = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "analysis_results",
                columns: table => new
                {
                    analysis_id = table.Column<string>(type: "text", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    site_id = table.Column<string>(type: "text", nullable: false),
                    user_goal = table.Column<string>(type: "text", nullable: false),
                    category = table.Column<List<string>>(type: "text[]", nullable: false),
                    intrinsic_score = table.Column<int>(type: "integer", nullable: false),
                    relevance_score = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_analysis_results", x => x.analysis_id);
                    table.ForeignKey(
                        name: "fk_analysis_results_sites_site_id",
                        column: x => x.site_id,
                        principalTable: "sites",
                        principalColumn: "site_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "game_stats",
                columns: table => new
                {
                    stat_id = table.Column<string>(type: "text", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    coin = table.Column<int>(type: "integer", nullable: false),
                    experience_points = table.Column<float>(type: "real", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_game_stats", x => x.stat_id);
                    table.ForeignKey(
                        name: "fk_game_stats_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_site_visits",
                columns: table => new
                {
                    visit_id = table.Column<string>(type: "text", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    site_id = table.Column<string>(type: "text", nullable: true),
                    analysis_id = table.Column<string>(type: "text", nullable: true),
                    visit_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_site_visits", x => x.visit_id);
                    table.ForeignKey(
                        name: "fk_user_site_visits_analysis_results_analysis_id",
                        column: x => x.analysis_id,
                        principalTable: "analysis_results",
                        principalColumn: "analysis_id");
                    table.ForeignKey(
                        name: "fk_user_site_visits_sites_site_id",
                        column: x => x.site_id,
                        principalTable: "sites",
                        principalColumn: "site_id");
                    table.ForeignKey(
                        name: "fk_user_site_visits_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_analysis_results_site_id_user_goal",
                table: "analysis_results",
                columns: new[] { "site_id", "user_goal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_game_stats_user_id",
                table: "game_stats",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_sites_url",
                table: "sites",
                column: "url",
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
                name: "ix_user_site_visits_user_id",
                table: "user_site_visits",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_site_visits_visit_date",
                table: "user_site_visits",
                column: "visit_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "game_stats");

            migrationBuilder.DropTable(
                name: "user_site_visits");

            migrationBuilder.DropTable(
                name: "analysis_results");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "sites");
        }
    }
}
