using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Timeline.Tests.Helpers
{
    public static class WebApplicationFactoryExtensions
    {
        public static WebApplicationFactory<TEntry> WithTestConfig<TEntry>(this WebApplicationFactory<TEntry> factory, ITestOutputHelper outputHelper) where TEntry : class
        {
            return factory.WithWebHostBuilder(builder =>
            {
                builder
                    .UseEnvironment(EnvironmentConstants.TestEnvironmentName)
                    .ConfigureLogging(logging =>
                    {
                        logging.AddXunit(outputHelper);
                    });
            });
        }
    }
}
