using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

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
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(maxLength: 26, nullable: false),
                    password = table.Column<string>(nullable: false),
                    roles = table.Column<string>(nullable: false),
                    version = table.Column<long>(nullable: false, defaultValue: 0L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_avatars",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    data = table.Column<byte[]>(nullable: true),
                    type = table.Column<string>(nullable: true),
                    etag = table.Column<string>(maxLength: 30, nullable: true),
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
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nickname = table.Column<string>(maxLength: 26, nullable: true),
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
                name: "user_avatars");

            migrationBuilder.DropTable(
                name: "user_details");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
