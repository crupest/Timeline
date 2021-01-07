using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Migrations;
using Timeline.Services;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests.Helpers
{
    public class TestDatabase : IAsyncLifetime
    {
        private readonly bool _createUser;

        public TestDatabase(bool createUser = true)
        {
            _createUser = createUser;
            Connection = new SqliteConnection("Data Source=:memory:;");
        }

        public async Task InitializeAsync()
        {
            await Connection.OpenAsync();

            using (var context = CreateContext())
            {
                await context.Database.EnsureCreatedAsync();
                context.JwtToken.Add(new JwtTokenEntity
                {
                    Key = JwtTokenGenerateHelper.GenerateKey()
                });

                await context.SaveChangesAsync();

                var passwordService = new PasswordService();
                var userService = new UserService(NullLogger<UserService>.Instance, context, passwordService, new Clock());

                var admin = await userService.CreateUser("admin", "adminpw");
                await userService.ModifyUser(admin.Id, new ModifyUserParams() { Nickname = "administrator" });

                admin.Permissions.Add(new UserPermissionEntity { Permission = UserPermission.AllTimelineManagement.ToString() });
                admin.Permissions.Add(new UserPermissionEntity { Permission = UserPermission.HighlightTimelineManagement.ToString() });
                admin.Permissions.Add(new UserPermissionEntity { Permission = UserPermission.UserManagement.ToString() });
                await context.SaveChangesAsync();

                if (_createUser)
                {
                    var user = await userService.CreateUser("user", "userpw");
                    await userService.ModifyUser(user.Id, new ModifyUserParams() { Nickname = "imuser" });
                }
            }
        }

        public async Task DisposeAsync()
        {
            await Connection.CloseAsync();
            await Connection.DisposeAsync();
        }

        public SqliteConnection Connection { get; }

        public DatabaseContext CreateContext(ITestOutputHelper? testOutputHelper = null)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlite(Connection);

            if (testOutputHelper != null) optionsBuilder.LogTo(testOutputHelper.WriteLine).EnableDetailedErrors().EnableSensitiveDataLogging();

            return new DatabaseContext(optionsBuilder.Options);
        }
    }
}
