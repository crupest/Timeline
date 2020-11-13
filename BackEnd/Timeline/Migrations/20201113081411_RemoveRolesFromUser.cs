using Microsoft.EntityFrameworkCore.Migrations;

namespace Timeline.Migrations
{
    public partial class RemoveRolesFromUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "roles",
                table: "users");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "roles",
                table: "users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
