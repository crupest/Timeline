using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timeline.Migrations
{
    public partial class AddDeletedToToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "deleted",
                table: "user_token",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deleted",
                table: "user_token");
        }
    }
}
