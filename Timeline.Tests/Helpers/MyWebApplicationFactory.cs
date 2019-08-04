using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Timeline.Models;
using Timeline.Services;
using Xunit.Abstractions;

namespace Timeline.Tests.Helpers
{
    public class MyWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        // We should keep the connection, so the database is persisted but not recreate every time.
        // See https://docs.microsoft.com/en-us/ef/core/miscellaneous/testing/sqlite#writing-tests .
        private readonly SqliteConnection _databaseConnection;

        public MyWebApplicationFactory() : base()
        {
            _databaseConnection = new SqliteConnection("Data Source=:memory:;");
            _databaseConnection.Open();

            InitDatabase();
        }

        private void InitDatabase()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                    .UseSqlite(_databaseConnection)
                    .Options;

            using (var context = new DatabaseContext(options))
            {
                context.Database.EnsureCreated();
                context.Users.AddRange(TestMockUsers.MockUsers);
                context.SaveChanges();
            }
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddEntityFrameworkSqlite();
                services.AddDbContext<DatabaseContext>(options =>
                {
                    options.UseSqlite(_databaseConnection);
                });
            })
            .ConfigureTestServices(services =>
            {
                services.AddSingleton<IClock, TestClock>();
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _databaseConnection.Close();
                _databaseConnection.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    public static class WebApplicationFactoryExtensions
    {
        public static WebApplicationFactory<TEntry> WithTestLogging<TEntry>(this WebApplicationFactory<TEntry> factory, ITestOutputHelper outputHelper) where TEntry : class
        {
            return factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureLogging(logging =>
                {
                    logging.AddXunit(outputHelper);
                });
            });
        }
    }
}
