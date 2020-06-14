using Microsoft.EntityFrameworkCore.Migrations;

namespace Timeline.Migrations
{
    public partial class AddTimelineUniqueId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
@"
ALTER TABLE timelines RENAME TO timelines_backup;
ALTER TABLE timeline_members RENAME TO timeline_members_backup;
ALTER TABLE timeline_posts RENAME TO timeline_posts_backup;

CREATE TABLE timelines (
    id INTEGER NOT NULL CONSTRAINT PK_timelines PRIMARY KEY AUTOINCREMENT,
	unique_id TEXT NOT NULL DEFAULT (lower(hex(randomblob(16)))),
    name TEXT NULL,
    description TEXT NULL,
    owner INTEGER NOT NULL,
    visibility INTEGER NOT NULL,
    create_time TEXT NOT NULL, current_post_local_id INTEGER NOT NULL DEFAULT 0,
    CONSTRAINT FK_timelines_users_owner FOREIGN KEY (owner) REFERENCES users (id) ON DELETE CASCADE
);

CREATE TABLE timeline_members (
    id       INTEGER NOT NULL
                     CONSTRAINT PK_timeline_members PRIMARY KEY AUTOINCREMENT,
    user     INTEGER NOT NULL,
    timeline INTEGER NOT NULL,
    CONSTRAINT FK_timeline_members_timelines_timeline FOREIGN KEY (
        timeline
    )
    REFERENCES timelines (id) ON DELETE CASCADE,
    CONSTRAINT FK_timeline_members_users_user FOREIGN KEY (
        user
    )
    REFERENCES users (id) ON DELETE CASCADE
);

CREATE TABLE timeline_posts (
    id            INTEGER NOT NULL
                          CONSTRAINT PK_timeline_posts PRIMARY KEY AUTOINCREMENT,
    timeline      INTEGER NOT NULL,
    author        INTEGER NOT NULL,
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
    REFERENCES users (id) ON DELETE CASCADE,
    CONSTRAINT FK_timeline_posts_timelines_timeline FOREIGN KEY (
        timeline
    )
    REFERENCES timelines (id) ON DELETE CASCADE
);


INSERT INTO timelines (id, name, description, owner, visibility, create_time)
	SELECT id, name, description, owner, visibility, create_time FROM timelines_backup;
INSERT INTO timeline_members SELECT * FROM timeline_members_backup;
INSERT INTO timeline_posts SELECT * FROM timeline_posts_backup;
	
DROP TABLE timelines_backup;
DROP TABLE timeline_members_backup;
DROP TABLE timeline_posts_backup;
"
                );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
