using Microsoft.EntityFrameworkCore.Migrations;

namespace Timeline.Migrations.DevelopmentDatabase
{
    public partial class RefactorUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "name", table: "users", newName: "username");
            migrationBuilder.RenameIndex(name: "IX_users_name", table: "users", newName: "IX_users_username");

            migrationBuilder.AddColumn<string>(
                name: "nickname",
                table: "users",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE users 
    SET nickname = (
        SELECT nickname
        FROM user_details
        WHERE user_details.UserId = users.id
    );
            ");

            /*
            migrationBuilder.RenameColumn(name: "UserId", table: "user_avatars", newName: "user");

            migrationBuilder.DropForeignKey(
                name: "FK_user_avatars_users_UserId",
                table: "user_avatars");

            migrationBuilder.AddForeignKey(
                name: "FK_user_avatars_users_user",
                table: "user_avatars",
                column: "user",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

             migrationBuilder.RenameIndex(
                name: "IX_user_avatars_UserId",
                table: "user_avatars",
                newName: "IX_user_avatars_user");
             */

            migrationBuilder.Sql(@"
CREATE TABLE user_avatars_backup (
    id            INTEGER NOT NULL
                          CONSTRAINT PK_user_avatars PRIMARY KEY AUTOINCREMENT,
    data          BLOB,
    type          TEXT,
    etag          TEXT,
    last_modified TEXT    NOT NULL,
    user          INTEGER NOT NULL,
    CONSTRAINT FK_user_avatars_users_user FOREIGN KEY (
        user
    )
    REFERENCES users (id) ON DELETE CASCADE
);

INSERT INTO user_avatars_backup (id, data, type, etag, last_modified, user)
    SELECT id, data, type, etag, last_modified, UserId FROM user_avatars;

DROP TABLE user_avatars;

ALTER TABLE user_avatars_backup
    RENAME TO user_avatars;

CREATE UNIQUE INDEX IX_user_avatars_user ON user_avatars (user);
            ");

            // migrationBuilder.DropTable(name: "user_details");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TABLE user_avatars_backup (
    id            INTEGER NOT NULL
                          CONSTRAINT PK_user_avatars PRIMARY KEY AUTOINCREMENT,
    data          BLOB,
    type          TEXT,
    etag          TEXT,
    last_modified TEXT    NOT NULL,
    UserId        INTEGER NOT NULL,
    CONSTRAINT FK_user_avatars_users_UserId FOREIGN KEY (
        user
    )
    REFERENCES users (id) ON DELETE CASCADE
);

INSERT INTO user_avatars_backup (id, data, type, etag, last_modified, UserId)
    SELECT id, data, type, etag, last_modified, user FROM user_avatars;

DROP TABLE user_avatars;

ALTER TABLE user_avatars_backup
    RENAME TO user_avatars;

CREATE UNIQUE INDEX IX_user_avatars_UserId ON user_avatars (UserId);
            ");

            migrationBuilder.Sql(@"
CREATE TABLE users_backup (
    id       INTEGER NOT NULL
                     CONSTRAINT PK_users PRIMARY KEY AUTOINCREMENT,
    name     TEXT    NOT NULL,
    password TEXT    NOT NULL,
    roles    TEXT    NOT NULL,
    version  INTEGER NOT NULL
                     DEFAULT 0
);

INSERT INTO users_backup (id, name, password, roles, version)
    SELECT id, username, password, roles, version FROM users;

DROP TABLE users;

ALTER TABLE users_backup
    RENAME TO users;

CREATE UNIQUE INDEX IX_users_name ON users (name);
            ");
        }
    }
}
