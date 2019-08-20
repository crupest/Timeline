using Microsoft.EntityFrameworkCore.Migrations;

namespace Timeline.Migrations
{
    public partial class MakeUserNameIndexUnique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_name",
                table: "users");

            migrationBuilder.CreateIndex(
                name: "IX_users_name",
                table: "users",
                column: "name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_name",
                table: "users");

            migrationBuilder.CreateIndex(
                name: "IX_users_name",
                table: "users",
                column: "name");
        }
    }
}
