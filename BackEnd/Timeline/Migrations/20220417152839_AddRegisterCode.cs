using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timeline.Migrations
{
    public partial class AddRegisterCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "register_code",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    owner_id = table.Column<long>(type: "INTEGER", nullable: true),
                    code = table.Column<string>(type: "TEXT", nullable: false),
                    enabled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_register_code", x => x.id);
                    table.ForeignKey(
                        name: "FK_register_code_users_owner_id",
                        column: x => x.owner_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "user_register_info",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<long>(type: "INTEGER", nullable: false),
                    register_code = table.Column<string>(type: "TEXT", nullable: false),
                    introducer_id = table.Column<long>(type: "INTEGER", nullable: true),
                    register_time = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_register_info", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_register_info_users_introducer_id",
                        column: x => x.introducer_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_user_register_info_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_register_code_owner_id",
                table: "register_code",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_register_info_introducer_id",
                table: "user_register_info",
                column: "introducer_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_register_info_user_id",
                table: "user_register_info",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "register_code");

            migrationBuilder.DropTable(
                name: "user_register_info");
        }
    }
}
