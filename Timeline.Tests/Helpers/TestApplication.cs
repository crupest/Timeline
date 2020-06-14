using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

        public IHost Host { get; private set; }

        public string WorkDir { get; private set; }

        public TestApplication()
        {

        }

        public async Task InitializeAsync()
        {
            WorkDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(WorkDir);

            DatabaseConnection = new SqliteConnection("Data Source=:memory:;");
            await DatabaseConnection.OpenAsync();

            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlite(DatabaseConnection).Options;

            using (var context = new DatabaseContext(options))
            {
                await context.Database.EnsureCreatedAsync();
                context.JwtToken.Add(new JwtTokenEntity
                {
                    Key = JwtTokenGenerateHelper.GenerateKey()
                });
                await context.SaveChangesAsync();
            }

            Host = await Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        [ApplicationConfiguration.UseMockFrontEndKey] = "true",
                        ["WorkDir"] = WorkDir
                    });
                })
                .ConfigureServices(services =>
                {
                    services.AddDbContext<DatabaseContext>(options =>
                    {
                        options.UseSqlite(DatabaseConnection);
                    });
                })
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder
                        .UseTestServer()
                        .UseStartup<Startup>();
                })
                .StartAsync();
        }

        public async Task DisposeAsync()
        {
            await Host.StopAsync();
            Host.Dispose();

            await DatabaseConnection.CloseAsync();
            await DatabaseConnection.DisposeAsync();
            Directory.Delete(WorkDir, true);
        }
    }
}
