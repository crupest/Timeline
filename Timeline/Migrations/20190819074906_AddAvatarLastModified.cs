using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Timeline.Migrations
{
    public partial class AddAvatarLastModified : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_user_avatars_AvatarId",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_AvatarId",
                table: "users");

            migrationBuilder.DropColumn(
                name: "AvatarId",
                table: "users");

            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "user_avatars",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<byte[]>(
                name: "data",
                table: "user_avatars",
                nullable: true,
                oldClrType: typeof(byte[]));

            migrationBuilder.AddColumn<DateTime>(
                name: "last_modified",
                table: "user_avatars",
                nullable: false,
                defaultValue: DateTime.Now);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "user_avatars",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_user_avatars_UserId",
                table: "user_avatars",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_user_avatars_users_UserId",
                table: "user_avatars",
                column: "UserId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            // Note! Remember to manually create avatar entities for all users.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_avatars_users_UserId",
                table: "user_avatars");

            migrationBuilder.DropIndex(
                name: "IX_user_avatars_UserId",
                table: "user_avatars");

            migrationBuilder.DropColumn(
                name: "last_modified",
                table: "user_avatars");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "user_avatars");

            migrationBuilder.AddColumn<long>(
                name: "AvatarId",
                table: "users",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "user_avatars",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "data",
                table: "user_avatars",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldNullable: true);

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
    }
}
