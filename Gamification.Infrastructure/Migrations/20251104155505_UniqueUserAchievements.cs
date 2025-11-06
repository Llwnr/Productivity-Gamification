using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gamification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UniqueUserAchievements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_achievements_user_id",
                table: "user_achievements");

            migrationBuilder.CreateIndex(
                name: "ix_user_achievements_user_id_achievement_id",
                table: "user_achievements",
                columns: new[] { "user_id", "achievement_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_achievements_user_id_achievement_id",
                table: "user_achievements");

            migrationBuilder.CreateIndex(
                name: "ix_user_achievements_user_id",
                table: "user_achievements",
                column: "user_id");
        }
    }
}
