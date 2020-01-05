using Microsoft.EntityFrameworkCore.Migrations;

namespace Timeline.Migrations.ProductionDatabase
{
    public partial class RenameTimelineMember : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimelineMembers_timelines_timeline",
                table: "TimelineMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_TimelineMembers_users_user",
                table: "TimelineMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TimelineMembers",
                table: "TimelineMembers");

            migrationBuilder.RenameTable(
                name: "TimelineMembers",
                newName: "timeline_members");

            migrationBuilder.RenameIndex(
                name: "IX_TimelineMembers_user",
                table: "timeline_members",
                newName: "IX_timeline_members_user");

            migrationBuilder.RenameIndex(
                name: "IX_TimelineMembers_timeline",
                table: "timeline_members",
                newName: "IX_timeline_members_timeline");

            migrationBuilder.AddPrimaryKey(
                name: "PK_timeline_members",
                table: "timeline_members",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_timeline_members_timelines_timeline",
                table: "timeline_members",
                column: "timeline",
                principalTable: "timelines",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_timeline_members_users_user",
                table: "timeline_members",
                column: "user",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_timeline_members_timelines_timeline",
                table: "timeline_members");

            migrationBuilder.DropForeignKey(
                name: "FK_timeline_members_users_user",
                table: "timeline_members");

            migrationBuilder.DropPrimaryKey(
                name: "PK_timeline_members",
                table: "timeline_members");

            migrationBuilder.RenameTable(
                name: "timeline_members",
                newName: "TimelineMembers");

            migrationBuilder.RenameIndex(
                name: "IX_timeline_members_user",
                table: "TimelineMembers",
                newName: "IX_TimelineMembers_user");

            migrationBuilder.RenameIndex(
                name: "IX_timeline_members_timeline",
                table: "TimelineMembers",
                newName: "IX_TimelineMembers_timeline");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TimelineMembers",
                table: "TimelineMembers",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_TimelineMembers_timelines_timeline",
                table: "TimelineMembers",
                column: "timeline",
                principalTable: "timelines",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimelineMembers_users_user",
                table: "TimelineMembers",
                column: "user",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
