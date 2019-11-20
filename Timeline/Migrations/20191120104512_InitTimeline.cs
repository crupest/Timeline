using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Timeline.Migrations
{
    public partial class InitTimeline : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "roles",
                table: "users",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "users",
                maxLength: 26,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(26)",
                oldMaxLength: 26);

            migrationBuilder.AlterColumn<string>(
                name: "password",
                table: "users",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.AlterColumn<string>(
                name: "nickname",
                table: "user_details",
                maxLength: 26,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(26)",
                oldMaxLength: 26,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "user_avatars",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "etag",
                table: "user_avatars",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "timelines",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    owner = table.Column<long>(nullable: false),
                    visibility = table.Column<int>(nullable: false),
                    create_time = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_timelines", x => x.id);
                    table.ForeignKey(
                        name: "FK_timelines_users_owner",
                        column: x => x.owner,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "timeline_posts",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    timeline = table.Column<long>(nullable: false),
                    author = table.Column<long>(nullable: false),
                    content = table.Column<string>(nullable: true),
                    time = table.Column<DateTime>(nullable: false),
                    last_updated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_timeline_posts", x => x.id);
                    table.ForeignKey(
                        name: "FK_timeline_posts_users_author",
                        column: x => x.author,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_timeline_posts_timelines_timeline",
                        column: x => x.timeline,
                        principalTable: "timelines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimelineMembers",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user = table.Column<long>(nullable: false),
                    timeline = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimelineMembers", x => x.id);
                    table.ForeignKey(
                        name: "FK_TimelineMembers_timelines_timeline",
                        column: x => x.timeline,
                        principalTable: "timelines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimelineMembers_users_user",
                        column: x => x.user,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_timeline_posts_author",
                table: "timeline_posts",
                column: "author");

            migrationBuilder.CreateIndex(
                name: "IX_timeline_posts_timeline",
                table: "timeline_posts",
                column: "timeline");

            migrationBuilder.CreateIndex(
                name: "IX_TimelineMembers_timeline",
                table: "TimelineMembers",
                column: "timeline");

            migrationBuilder.CreateIndex(
                name: "IX_TimelineMembers_user",
                table: "TimelineMembers",
                column: "user");

            migrationBuilder.CreateIndex(
                name: "IX_timelines_owner",
                table: "timelines",
                column: "owner");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "timeline_posts");

            migrationBuilder.DropTable(
                name: "TimelineMembers");

            migrationBuilder.DropTable(
                name: "timelines");

            migrationBuilder.AlterColumn<string>(
                name: "roles",
                table: "users",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "users",
                type: "varchar(26)",
                maxLength: 26,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 26);

            migrationBuilder.AlterColumn<string>(
                name: "password",
                table: "users",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "nickname",
                table: "user_details",
                type: "varchar(26)",
                maxLength: 26,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 26,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "user_avatars",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "etag",
                table: "user_avatars",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 30,
                oldNullable: true);
        }
    }
}
