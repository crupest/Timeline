using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Timeline.Configs;
using Timeline.Entities;
using Timeline.Migrations;
using Xunit;

namespace Timeline.Tests.Helpers
{
    public class TestApplication : IAsyncLifetime
    {
        public SqliteConnection DatabaseConnection { get; private set; }

        public WebApplicationFactory<Startup> Factory { get; private set; }

        public string WorkDir { get; private set; }

        public TestApplication(WebApplicationFactory<Startup> factory)
        {
            Factory = factory;
        }

        public async Task InitializeAsync()
        {
            WorkDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(WorkDir);

            DatabaseConnection = new SqliteConnection("Data Source=:memory:;");
            await DatabaseConnection.OpenAsync();

            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlite(DatabaseConnection)
                .Options;

            using (var context = new DatabaseContext(options))
            {
                await context.Database.EnsureCreatedAsync();
                context.JwtToken.Add(new JwtTokenEntity
                {
                    Key = JwtTokenGenerateHelper.GenerateKey()
                });
                await context.SaveChangesAsync();
            }

            Factory = Factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        [ApplicationConfiguration.DisableFrontEndKey] = "true",
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

        public async Task DisposeAsync()
        {
            await DatabaseConnection.CloseAsync();
            await DatabaseConnection.DisposeAsync();
            Directory.Delete(WorkDir, true);
        }
    }
}
