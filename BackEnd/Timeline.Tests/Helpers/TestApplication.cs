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
using Xunit;

namespace Timeline.Tests.Helpers
{
    public class TestApplication : IAsyncLifetime
    {
        public TestDatabase Database { get; }

        public IHost Host { get; private set; }

        public string WorkDir { get; private set; }

        public TestApplication()
        {
            Database = new TestDatabase(false);
        }

        public async Task InitializeAsync()
        {
            await Database.InitializeAsync();

            WorkDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(WorkDir);

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
                        options.UseSqlite(Database.Connection);
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

            Directory.Delete(WorkDir, true);

            await Database.DisposeAsync();
        }
    }
}
