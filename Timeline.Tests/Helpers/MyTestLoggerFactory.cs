using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Xunit.Abstractions;

namespace Timeline.Tests.Helpers
{
    public static class MyTestLoggerFactory
    {
        public static LoggerFactory Create(ITestOutputHelper outputHelper)
        {
            return new LoggerFactory(new[] { new XunitLoggerProvider(outputHelper) });
        }
    }
}
