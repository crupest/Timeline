using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Timeline.Migrations.ProductionDatabase
{
    public partial class RefactorUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
START TRANSACTION;

ALTER TABLE `users`
    CHANGE COLUMN `name` `username` varchar (26) NOT NULL,
    RENAME INDEX IX_users_name TO IX_users_username,
    ADD `nickname` varchar(100) CHARACTER SET utf8mb4 NULL;

UPDATE users 
    SET nickname = (
        SELECT nickname
        FROM user_details
        WHERE user_details.UserId = users.id
    );

ALTER TABLE `user_avatars`
    CHANGE COLUMN `UserId` `user` bigint (20) NOT NULL,
    RENAME INDEX IX_user_avatars_UserId TO IX_user_avatars_user,
    DROP FOREIGN KEY FK_user_avatars_users_UserId,
    ADD CONSTRAINT FK_user_avatars_users_user FOREIGN KEY (`user`) REFERENCES `users` (`id`) ON DELETE CASCADE;

COMMIT;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
START TRANSACTION;

ALTER TABLE `users`
    CHANGE COLUMN `username` `name` varchar (26) NOT NULL,
    RENAME INDEX IX_users_username TO IX_users_name,
    DROP COLUMN `nickname`;

ALTER TABLE `user_avatars`
    CHANGE COLUMN `user` `UserId` bigint (20) NOT NULL,
    RENAME INDEX IX_user_avatars_user TO IX_user_avatars_UserId,
    DROP FOREIGN KEY FK_user_avatars_users_user,
    ADD CONSTRAINT FK_user_avatars_users_UserId FOREIGN KEY (`UserId`) REFERENCES `users` (`id`) ON DELETE CASCADE;

COMMIT;
            ");
        }
    }
}
