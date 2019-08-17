using Microsoft.EntityFrameworkCore.Migrations;

namespace Timeline.Migrations
{
    public partial class Enhance1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "version",
                table: "user",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "user",
                maxLength: 26,
                nullable: false,
                oldClrType: typeof(string));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "version",
                table: "user",
                nullable: false,
                oldClrType: typeof(long),
                oldDefaultValue: 0L);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "user",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 26);
        }
    }
}
