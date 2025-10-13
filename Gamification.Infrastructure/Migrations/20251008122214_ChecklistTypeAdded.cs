using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gamification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChecklistTypeAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "checklist",
                table: "tasks");

            migrationBuilder.AddColumn<string>(
                name: "dailies_checklist",
                table: "tasks",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "todo_checklist",
                table: "tasks",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "dailies_checklist",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "todo_checklist",
                table: "tasks");

            migrationBuilder.AddColumn<List<string>>(
                name: "checklist",
                table: "tasks",
                type: "text[]",
                nullable: true);
        }
    }
}
