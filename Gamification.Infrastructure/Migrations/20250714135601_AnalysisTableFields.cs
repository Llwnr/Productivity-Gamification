using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gamification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AnalysisTableFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "category",
                table: "analysis_results",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "intrinsic_score",
                table: "analysis_results",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "relevance_score",
                table: "analysis_results",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "user_goal",
                table: "analysis_results",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "visited_date",
                table: "analysis_results",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "category",
                table: "analysis_results");

            migrationBuilder.DropColumn(
                name: "intrinsic_score",
                table: "analysis_results");

            migrationBuilder.DropColumn(
                name: "relevance_score",
                table: "analysis_results");

            migrationBuilder.DropColumn(
                name: "user_goal",
                table: "analysis_results");

            migrationBuilder.DropColumn(
                name: "visited_date",
                table: "analysis_results");
        }
    }
}
