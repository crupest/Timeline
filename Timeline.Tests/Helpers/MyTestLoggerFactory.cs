using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;

namespace Timeline.Tests.Helpers
{
    public static class Logging
    {
        public static ILoggerFactory Create(ITestOutputHelper outputHelper)
        {
            // TODO: Use test output.
            return NullLoggerFactory.Instance;
        }

        public static IWebHostBuilder ConfigureTestLogging(this IWebHostBuilder builder)
        {
            builder.ConfigureLogging(logging =>
            {
                //logging.AddXunit(outputHelper);
            });
            return builder;
        }
    }
}
