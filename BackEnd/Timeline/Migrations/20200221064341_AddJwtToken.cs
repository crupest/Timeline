using Microsoft.EntityFrameworkCore.Migrations;
using System.Security.Cryptography;

namespace Timeline.Migrations
{
    public static class JwtTokenGenerateHelper
    {
        public static byte[] GenerateKey()
        {
            using var random = RandomNumberGenerator.Create();
            var key = new byte[16];
            random.GetBytes(key);
            return key;
        }
    }

    public partial class AddJwtToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "jwt_token",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    key = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jwt_token", x => x.id);
                });


            migrationBuilder.InsertData("jwt_token", "key", JwtTokenGenerateHelper.GenerateKey());
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "jwt_token");
        }
    }
}
