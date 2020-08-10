using Microsoft.EntityFrameworkCore.Migrations;

namespace Timeline.Migrations
{
    public partial class MakePostAuthorOptional : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;

CREATE TABLE new_timeline_posts (
    id            INTEGER NOT NULL
                          CONSTRAINT PK_timeline_posts PRIMARY KEY AUTOINCREMENT,
    timeline      INTEGER NOT NULL,
    author        INTEGER,
    content       TEXT,
    time          TEXT    NOT NULL,
    last_updated  TEXT    NOT NULL,
    local_id      INTEGER NOT NULL
                          DEFAULT 0,
    content_type  TEXT    NOT NULL
                          DEFAULT '',
    extra_content TEXT,
    CONSTRAINT FK_timeline_posts_users_author FOREIGN KEY (
        author
    )
    REFERENCES users (id),
    CONSTRAINT FK_timeline_posts_timelines_timeline FOREIGN KEY (
        timeline
    )
    REFERENCES timelines (id) ON DELETE CASCADE
);

INSERT INTO new_timeline_posts SELECT * FROM timeline_posts;

DROP TABLE timeline_posts;

ALTER TABLE new_timeline_posts RENAME TO timeline_posts; 

CREATE INDEX IX_timeline_posts_author ON timeline_posts (author);

CREATE INDEX IX_timeline_posts_timeline ON timeline_posts(timeline);

PRAGMA foreign_key_check;

COMMIT TRANSACTION;

PRAGMA foreign_keys = 1;
            ", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_timeline_posts_users_author",
                table: "timeline_posts");

            migrationBuilder.AlterColumn<long>(
                name: "author",
                table: "timeline_posts",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_timeline_posts_users_author",
                table: "timeline_posts",
                column: "author",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
