using System;
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

CREATE TABLE timelines (
    id INTEGER NOT NULL CONSTRAINT PK_timelines PRIMARY KEY AUTOINCREMENT,
	unique_id BLOB NOT NULL DEFAULT (randomblob(16)),
    name TEXT NULL,
    description TEXT NULL,
    owner INTEGER NOT NULL,
    visibility INTEGER NOT NULL,
    create_time TEXT NOT NULL, current_post_local_id INTEGER NOT NULL DEFAULT 0,
    CONSTRAINT FK_timelines_users_owner FOREIGN KEY (owner) REFERENCES users (id) ON DELETE CASCADE
);

INSERT INTO timelines (id, name, description, owner, visibility, create_time)
	SELECT id, name, description, owner, visibility, create_time FROM timelines_backup;

DROP TABLE timelines_backup;
"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
