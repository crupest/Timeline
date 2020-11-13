using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;

namespace Timeline.Migrations
{
    public partial class TimelineAddModifiedTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var currentTime = new DateTimeToStringConverter().ConvertToProvider(DateTime.Now);

            migrationBuilder.Sql(
@$"
PRAGMA foreign_keys=OFF;

BEGIN TRANSACTION;

CREATE TABLE new_timelines (
    id INTEGER NOT NULL CONSTRAINT PK_timelines PRIMARY KEY AUTOINCREMENT,
	unique_id TEXT NOT NULL DEFAULT (lower(hex(randomblob(16)))),
    name TEXT NULL,
    name_last_modified TEXT NOT NULL,
    description TEXT NULL,
    owner INTEGER NOT NULL,
    visibility INTEGER NOT NULL,
    create_time TEXT NOT NULL,
    last_modified TEXT NOT NULL,
    current_post_local_id INTEGER NOT NULL DEFAULT 0,
    CONSTRAINT FK_timelines_users_owner FOREIGN KEY (owner) REFERENCES users (id) ON DELETE CASCADE
);

INSERT INTO new_timelines (id, unique_id, name, name_last_modified, description, owner, visibility, create_time, last_modified, current_post_local_id)
	SELECT id, unique_id, name, '{currentTime}', description, owner, visibility, create_time, '{currentTime}', current_post_local_id FROM timelines;
	
DROP TABLE timelines;

ALTER TABLE new_timelines
	RENAME TO timelines; 

CREATE INDEX IX_timelines_owner ON timelines (owner);

PRAGMA foreign_key_check;

COMMIT TRANSACTION;

PRAGMA foreign_keys=ON;
"
            , true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
