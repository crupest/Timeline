using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Timeline.Configs;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests.Helpers
{
    public class TestApplication : IAsyncLifetime
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public TestApplication(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public IHost Host { get; private set; } = default!;

        public string WorkDirectory { get; private set; } = default!;

        public async Task InitializeAsync()
        {
            WorkDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(WorkDirectory);

            Host = await Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddXUnit(_testOutputHelper);
                })
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

        public TestServer Server
        {
            get => (TestServer)Host.Services.GetRequiredService<IServer>();
        }
    }
}
