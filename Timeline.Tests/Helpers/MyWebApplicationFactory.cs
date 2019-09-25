using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Timeline.Entities;
using Timeline.Services;
using Timeline.Tests.Mock.Data;
using Timeline.Tests.Mock.Services;
using Xunit.Abstractions;

namespace Timeline.Tests.Helpers
{
    public class MyWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IClock, TestClock>();
            });
        }
    }

    public static class WebApplicationFactoryExtensions
    {
        public static WebApplicationFactory<TEntry> WithTestConfig<TEntry>(this WebApplicationFactory<TEntry> factory, ITestOutputHelper outputHelper, out Action disposeAction) where TEntry : class
        {
            // We should keep the connection, so the database is persisted but not recreate every time.
            // See https://docs.microsoft.com/en-us/ef/core/miscellaneous/testing/sqlite#writing-tests .
            SqliteConnection _databaseConnection = new SqliteConnection("Data Source=:memory:;");
            _databaseConnection.Open();

            {
                var options = new DbContextOptionsBuilder<DatabaseContext>()
                    .UseSqlite(_databaseConnection)
                    .ConfigureWarnings(builder =>
                    {
                        builder.Throw(RelationalEventId.QueryClientEvaluationWarning);
                    })
                    .Options;

                using (var context = new DatabaseContext(options))
                {
                    TestDatabase.InitDatabase(context);
                };
            }

            disposeAction = () =>
            {
                _databaseConnection.Close();
                _databaseConnection.Dispose();
            };

            return factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestLogging()
                .ConfigureServices(services =>
                {
                    services.AddEntityFrameworkSqlite();
                    services.AddDbContext<DatabaseContext>(options =>
                    {
                        options.UseSqlite(_databaseConnection);
                    });
                });
            });
        }
    }
}
