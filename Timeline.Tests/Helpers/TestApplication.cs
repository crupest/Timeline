using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using Timeline.Entities;

namespace Timeline.Tests.Helpers
{
    public class TestApplication : IDisposable
    {
        public SqliteConnection DatabaseConnection { get; }

        public WebApplicationFactory<Startup> Factory { get; }

        public string WorkDir { get; }

        public TestApplication(WebApplicationFactory<Startup> factory)
        {
            WorkDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(WorkDir);

            DatabaseConnection = new SqliteConnection("Data Source=:memory:;");
            DatabaseConnection.Open();

            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlite(DatabaseConnection)
                .Options;

            using (var context = new DatabaseContext(options))
            {
                context.Database.EnsureCreated();
            }

            Factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["WorkDir"] = WorkDir
                    });
                });
                builder.ConfigureServices(services =>
                {
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

            Directory.Delete(WorkDir, true);
        }
    }
}
