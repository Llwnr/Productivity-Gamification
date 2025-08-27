using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gamification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedVisitEndDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "visit_date",
                table: "user_site_visits",
                newName: "visit_start_date");

            migrationBuilder.RenameIndex(
                name: "ix_user_site_visits_visit_date",
                table: "user_site_visits",
                newName: "ix_user_site_visits_visit_start_date");

            migrationBuilder.AddColumn<DateTime>(
                name: "visit_end_date",
                table: "user_site_visits",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "visit_end_date",
                table: "user_site_visits");

            migrationBuilder.RenameColumn(
                name: "visit_start_date",
                table: "user_site_visits",
                newName: "visit_date");

            migrationBuilder.RenameIndex(
                name: "ix_user_site_visits_visit_start_date",
                table: "user_site_visits",
                newName: "ix_user_site_visits_visit_date");
        }
    }
}
