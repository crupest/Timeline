using Microsoft.EntityFrameworkCore.Migrations;

namespace Timeline.Migrations
{
    public partial class AddImagePost : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "content_type",
                table: "timeline_posts",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "extra_content",
                table: "timeline_posts",
                nullable: true);

            migrationBuilder.Sql($@"
UPDATE timeline_posts
SET content_type = 'text';
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "content_type",
                table: "timeline_posts");

            migrationBuilder.DropColumn(
                name: "extra_content",
                table: "timeline_posts");
        }
    }
}
