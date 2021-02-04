using Microsoft.EntityFrameworkCore.Migrations;

namespace Timeline.Migrations
{
    public partial class AddColor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "color",
                table: "timelines",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "color",
                table: "timeline_posts",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "color",
                table: "timelines");

            migrationBuilder.DropColumn(
                name: "color",
                table: "timeline_posts");
        }
    }
}
