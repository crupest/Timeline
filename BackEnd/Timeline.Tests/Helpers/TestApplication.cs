using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Timeline.Configs;
using Xunit;

namespace Timeline.Tests.Helpers
{
    public class TestApplication : IAsyncLifetime
    {
        public IHost Host { get; private set; } = default!;

        public string WorkDirectory { get; private set; } = default!;

        public async Task InitializeAsync()
        {
            WorkDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(WorkDirectory);

            Host = await Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        [ApplicationConfiguration.FrontEndKey] = "Mock",
                        [ApplicationConfiguration.WorkDirectoryKey] = WorkDirectory
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

            Directory.Delete(WorkDirectory, true);
        }
    }
}
