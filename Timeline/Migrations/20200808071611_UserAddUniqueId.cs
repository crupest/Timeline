using Microsoft.EntityFrameworkCore.Migrations;

namespace Timeline.Migrations
{
    public partial class UserAddUniqueId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
@"
PRAGMA foreign_keys=OFF;

BEGIN TRANSACTION;

CREATE TABLE new_users (
    id       INTEGER NOT NULL
                     CONSTRAINT PK_users PRIMARY KEY AUTOINCREMENT,
	unique_id TEXT NOT NULL DEFAULT (lower(hex(randomblob(16)))),
    username TEXT    NOT NULL,
    password TEXT    NOT NULL,
    roles    TEXT    NOT NULL,
    version  INTEGER NOT NULL
                     DEFAULT 0,
    nickname TEXT
);

INSERT INTO new_users (id, username, password, roles, version, nickname)
	SELECT id, username, password, roles, version, nickname FROM users;
	
DROP TABLE users;

ALTER TABLE new_users
	RENAME TO users; 

CREATE UNIQUE INDEX IX_users_username ON users (
    username
);

PRAGMA foreign_key_check;

COMMIT TRANSACTION;

PRAGMA foreign_keys=ON;
"
    , true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "unique_id",
                table: "users");
        }
    }
}
