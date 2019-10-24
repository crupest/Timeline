using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using Timeline.Entities;
using Timeline.Tests.Mock.Data;

namespace Timeline.Tests.Helpers
{
    public class TestApplication : IDisposable
    {
        public SqliteConnection DatabaseConnection { get; } = new SqliteConnection("Data Source=:memory:;");
        public WebApplicationFactory<Startup> Factory { get; }

        public TestApplication(WebApplicationFactory<Startup> factory)
        {
            // We should keep the connection, so the database is persisted but not recreate every time.
            // See https://docs.microsoft.com/en-us/ef/core/miscellaneous/testing/sqlite#writing-tests .
            DatabaseConnection.Open();

            {
                var options = new DbContextOptionsBuilder<DatabaseContext>()
                    .UseSqlite(DatabaseConnection)
                    .Options;

                using (var context = new DatabaseContext(options))
                {
                    TestDatabase.InitDatabase(context);
                };
            }

            Factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddEntityFrameworkSqlite();
                    services.AddDbContext<DatabaseContext>(options =>
                    {
                        options.UseSqlite(DatabaseConnection);
                    });
                });
            });
        }

        public void Dispose()
        {
            DatabaseConnection.Close();
            DatabaseConnection.Dispose();
        }
    }
}
