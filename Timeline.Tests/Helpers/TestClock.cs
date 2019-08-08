using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using Timeline.Services;

namespace Timeline.Tests.Helpers
{
    public class TestClock : IClock
    {
        public DateTime? MockCurrentTime { get; set; } = null;

        public DateTime GetCurrentTime()
        {
            return MockCurrentTime.GetValueOrDefault(DateTime.Now);
        }
    }

    public static class TestClockWebApplicationFactoryExtensions
    {
        public static TestClock GetTestClock<T>(this WebApplicationFactory<T> factory) where T : class
        {
            return factory.Server.Host.Services.GetRequiredService<IClock>() as TestClock;
        }
    }
}
