using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gamification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ArrayGamestatToSinglePerUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_game_stats_user_id",
                table: "game_stats");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "daily_target_hours",
                table: "users",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.CreateIndex(
                name: "ix_game_stats_user_id",
                table: "game_stats",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_game_stats_user_id",
                table: "game_stats");

            migrationBuilder.DropColumn(
                name: "daily_target_hours",
                table: "users");

            migrationBuilder.CreateIndex(
                name: "ix_game_stats_user_id",
                table: "game_stats",
                column: "user_id");
        }
    }
}
