using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Timeline.Migrations
{
    public partial class AddHighlightTimelines : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "highlight_timelines",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    timeline_id = table.Column<long>(type: "INTEGER", nullable: false),
                    operator_id = table.Column<long>(type: "INTEGER", nullable: true),
                    add_time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    order = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_highlight_timelines", x => x.id);
                    table.ForeignKey(
                        name: "FK_highlight_timelines_timelines_timeline_id",
                        column: x => x.timeline_id,
                        principalTable: "timelines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_highlight_timelines_users_operator_id",
                        column: x => x.operator_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_highlight_timelines_operator_id",
                table: "highlight_timelines",
                column: "operator_id");

            migrationBuilder.CreateIndex(
                name: "IX_highlight_timelines_timeline_id",
                table: "highlight_timelines",
                column: "timeline_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "highlight_timelines");
        }
    }
}
