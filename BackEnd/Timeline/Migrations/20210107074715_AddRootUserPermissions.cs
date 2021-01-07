using Microsoft.EntityFrameworkCore.Migrations;
using Timeline.Services;

namespace Timeline.Migrations
{
    public partial class AddRootUserPermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData("user_permission", new string[] { "user_id", "permission" }, new object[] { 1, UserPermission.UserManagement.ToString() });
            migrationBuilder.InsertData("user_permission", new string[] { "user_id", "permission" }, new object[] { 1, UserPermission.AllTimelineManagement.ToString() });
            migrationBuilder.InsertData("user_permission", new string[] { "user_id", "permission" }, new object[] { 1, UserPermission.HighlightTimelineManagement.ToString() });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
