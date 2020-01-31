using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using Timeline.Entities;

namespace Timeline.Tests.Helpers
{
    public class TestApplication : IDisposable
    {
        public SqliteConnection DatabaseConnection { get; }

        public WebApplicationFactory<Startup> Factory { get; }

        public TestApplication(WebApplicationFactory<Startup> factory)
        {
            DatabaseConnection = new SqliteConnection("Data Source=:memory:;");
            DatabaseConnection.Open();

            var options = new DbContextOptionsBuilder<DevelopmentDatabaseContext>()
                .UseSqlite(DatabaseConnection)
                .Options;

            using (var context = new DevelopmentDatabaseContext(options))
            {
                context.Database.EnsureCreated();
            }

            Factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddDbContext<DatabaseContext, DevelopmentDatabaseContext>(options =>
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
