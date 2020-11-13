using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Timeline.Migrations
{
    public partial class Initialize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(nullable: false),
                    password = table.Column<string>(nullable: false),
                    roles = table.Column<string>(nullable: false),
                    version = table.Column<long>(nullable: false, defaultValue: 0L)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "timelines",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
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
                name: "user_avatars",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    data = table.Column<byte[]>(nullable: true),
                    type = table.Column<string>(nullable: true),
                    etag = table.Column<string>(nullable: true),
                    last_modified = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_avatars", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_avatars_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_details",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nickname = table.Column<string>(nullable: true),
                    UserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_details", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_details_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "timeline_members",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user = table.Column<long>(nullable: false),
                    timeline = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_timeline_members", x => x.id);
                    table.ForeignKey(
                        name: "FK_timeline_members_timelines_timeline",
                        column: x => x.timeline,
                        principalTable: "timelines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_timeline_members_users_user",
                        column: x => x.user,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "timeline_posts",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
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

            migrationBuilder.CreateIndex(
                name: "IX_timeline_members_timeline",
                table: "timeline_members",
                column: "timeline");

            migrationBuilder.CreateIndex(
                name: "IX_timeline_members_user",
                table: "timeline_members",
                column: "user");

            migrationBuilder.CreateIndex(
                name: "IX_timeline_posts_author",
                table: "timeline_posts",
                column: "author");

            migrationBuilder.CreateIndex(
                name: "IX_timeline_posts_timeline",
                table: "timeline_posts",
                column: "timeline");

            migrationBuilder.CreateIndex(
                name: "IX_timelines_owner",
                table: "timelines",
                column: "owner");

            migrationBuilder.CreateIndex(
                name: "IX_user_avatars_UserId",
                table: "user_avatars",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_details_UserId",
                table: "user_details",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_name",
                table: "users",
                column: "name",
                unique: true);

            // Add a init user. Username is "administrator". Password is "crupest".
            migrationBuilder.InsertData("users", new string[] { "name", "password", "roles" },
                new object[] { "administrator", "AQAAAAEAACcQAAAAENsspZrk8Wo+UuMyg6QuWJsNvRg6gVu4K/TumVod3h9GVLX9zDVuQQds3o7V8QWJ2w==", "user,admin" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "timeline_members");

            migrationBuilder.DropTable(
                name: "timeline_posts");

            migrationBuilder.DropTable(
                name: "user_avatars");

            migrationBuilder.DropTable(
                name: "user_details");

            migrationBuilder.DropTable(
                name: "timelines");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
