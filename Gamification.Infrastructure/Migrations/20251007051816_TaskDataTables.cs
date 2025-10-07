using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gamification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TaskDataTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tasks",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    reward_points = table.Column<int>(type: "integer", nullable: true),
                    tag = table.Column<string>(type: "text", nullable: true),
                    discriminator = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    repeat_frequency = table.Column<string>(type: "text", nullable: true),
                    repeat_interval = table.Column<string>(type: "text", nullable: true),
                    repeat_every = table.Column<string>(type: "text", nullable: true),
                    checklist = table.Column<List<string>>(type: "text[]", nullable: true),
                    is_positive = table.Column<bool>(type: "boolean", nullable: true),
                    is_negative = table.Column<bool>(type: "boolean", nullable: true),
                    positive_count = table.Column<int>(type: "integer", nullable: true),
                    negative_count = table.Column<int>(type: "integer", nullable: true),
                    reset_interval = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_completed = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tasks", x => x.id);
                    table.ForeignKey(
                        name: "fk_tasks_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tasks_user_id",
                table: "tasks",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tasks");
        }
    }
}
