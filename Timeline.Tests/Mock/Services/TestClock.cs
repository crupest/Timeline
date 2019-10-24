using System;
using Timeline.Services;

namespace Timeline.Tests.Mock.Services
{
    public class TestClock : IClock
    {
        public DateTime? MockCurrentTime { get; set; } = null;

        public DateTime GetCurrentTime()
        {
            return MockCurrentTime.GetValueOrDefault(DateTime.Now);
        }
    }
}
