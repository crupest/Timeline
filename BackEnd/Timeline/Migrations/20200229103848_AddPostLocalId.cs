using Microsoft.EntityFrameworkCore.Migrations;

namespace Timeline.Migrations
{
    public partial class AddPostLocalId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<long>(
                name: "current_post_local_id",
                table: "timelines",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "local_id",
                table: "timeline_posts",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.Sql(@"
UPDATE timeline_posts
SET local_id = (SELECT COUNT (*)
                FROM timeline_posts AS p
                WHERE p.timeline =  timeline_posts.timeline
                    AND p.id  <= timeline_posts.id);

UPDATE timelines
SET current_post_local_id = (SELECT COUNT (*)
                             FROM timeline_posts AS p
                             WHERE p.timeline =  timelines.id);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
