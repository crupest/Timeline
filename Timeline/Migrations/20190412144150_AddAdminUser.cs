using Microsoft.EntityFrameworkCore.Migrations;
using Timeline.Services;

namespace Timeline.Migrations
{
    public partial class AddAdminUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData("user", new string[] { "name", "password", "roles" },
                new string[] { "crupest", new PasswordService().HashPassword("yang0101"), "user,admin" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData("user", "name", "crupest");
        }
    }
}
