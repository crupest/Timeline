using Microsoft.EntityFrameworkCore.Migrations;

namespace Timeline.Migrations
{
    public partial class DropUserDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "user_details");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
