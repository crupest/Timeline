using Microsoft.EntityFrameworkCore.Migrations;

namespace Timeline.Migrations
{
    public partial class AddBookmarkTimeline : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bookmark_timelines",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    timeline = table.Column<long>(type: "INTEGER", nullable: false),
                    user = table.Column<long>(type: "INTEGER", nullable: false),
                    rank = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookmark_timelines", x => x.id);
                    table.ForeignKey(
                        name: "FK_bookmark_timelines_timelines_timeline",
                        column: x => x.timeline,
                        principalTable: "timelines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bookmark_timelines_users_user",
                        column: x => x.user,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bookmark_timelines_timeline",
                table: "bookmark_timelines",
                column: "timeline");

            migrationBuilder.CreateIndex(
                name: "IX_bookmark_timelines_user",
                table: "bookmark_timelines",
                column: "user");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bookmark_timelines");
        }
    }
}
