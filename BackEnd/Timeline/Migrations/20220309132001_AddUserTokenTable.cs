using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timeline.Migrations
{
    public partial class AddUserTokenTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_token",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<long>(type: "INTEGER", nullable: false),
                    token = table.Column<string>(type: "TEXT", nullable: false),
                    expire_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    create_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_token", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_token_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_token_token",
                table: "user_token",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_token_user_id",
                table: "user_token",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_token");
        }
    }
}
