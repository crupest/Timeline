using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Timeline.Migrations
{
    public partial class AddUserAvatar : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AvatarId",
                table: "users",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "user_avatars",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    data = table.Column<byte[]>(nullable: false),
                    type = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_avatars", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_AvatarId",
                table: "users",
                column: "AvatarId");

            migrationBuilder.AddForeignKey(
                name: "FK_users_user_avatars_AvatarId",
                table: "users",
                column: "AvatarId",
                principalTable: "user_avatars",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_user_avatars_AvatarId",
                table: "users");

            migrationBuilder.DropTable(
                name: "user_avatars");

            migrationBuilder.DropIndex(
                name: "IX_users_AvatarId",
                table: "users");

            migrationBuilder.DropColumn(
                name: "AvatarId",
                table: "users");
        }
    }
}
