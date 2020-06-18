using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Services;

namespace Timeline.Tests.Helpers
{
    public class TestClock : IClock
    {
        private DateTime? _currentTime = null;

        public DateTime GetCurrentTime()
        {
            return _currentTime ?? DateTime.Now;
        }

        public void SetCurrentTime(DateTime? mockTime)
        {
            _currentTime = mockTime;
        }
    }
}
