using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TimelineApp.Migrations
{
    public partial class AddDataTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "data",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    tag = table.Column<string>(nullable: false),
                    data = table.Column<byte[]>(nullable: false),
                    @ref = table.Column<int>(name: "ref", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_data_tag",
                table: "data",
                column: "tag",
                unique: true);

            migrationBuilder.Sql(@"
ALTER TABLE user_avatars
    RENAME TO user_avatars_backup;

CREATE TABLE user_avatars (
    id            INTEGER NOT NULL
                          CONSTRAINT PK_user_avatars PRIMARY KEY AUTOINCREMENT,
    data_tag      TEXT,
    type          TEXT,
    last_modified TEXT    NOT NULL,
    user          INTEGER NOT NULL,
    CONSTRAINT FK_user_avatars_users_user FOREIGN KEY (
        user
    )
    REFERENCES users (id) ON DELETE CASCADE
);

INSERT INTO user_avatars (id, data_tag, type, last_modified, user)
    SELECT id, etag, type, last_modified, user FROM user_avatars_backup;

INSERT OR IGNORE INTO data (tag, data, ref)
    SELECT etag, data, 0 FROM user_avatars_backup;

UPDATE data
SET ref = (SELECT COUNT (*)
           FROM user_avatars_backup AS a
           WHERE a.etag == data.tag);

DROP TABLE user_avatars_backup;

CREATE UNIQUE INDEX IX_user_avatars_user ON user_avatars (user);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data");

            migrationBuilder.DropColumn(
                name: "data_tag",
                table: "user_avatars");

            migrationBuilder.AddColumn<byte[]>(
                name: "data",
                table: "user_avatars",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "etag",
                table: "user_avatars",
                type: "TEXT",
                nullable: true);
        }
    }
}
