using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Services;
using Timeline.Services.User;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests.Helpers
{
    public class TestDatabase : IAsyncLifetime
    {
        public TestDatabase()
        {
            Connection = new SqliteConnection("Data Source=:memory:;");
        }

        public async Task InitializeAsync()
        {
            await Connection.OpenAsync();

            using var context = CreateContext();
            await context.Database.MigrateAsync();

            var userService = new UserService(NullLogger<UserService>.Instance, context, new PasswordService(), new Clock());

            await userService.ModifyUserAsync(
                await userService.GetUserIdByUsernameAsync("administrator"),
                new ModifyUserParams() { Username = "admin", Password = "adminpw", Nickname = "administrator" });

            var user = await userService.CreateUserAsync(new CreateUserParams("user", "userpw"));
            await userService.ModifyUserAsync(user.Id, new ModifyUserParams() { Nickname = "imuser" });
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
