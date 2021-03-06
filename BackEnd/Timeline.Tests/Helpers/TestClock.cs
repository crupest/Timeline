﻿using System;
using Timeline.Services;

namespace Timeline.Tests.Helpers
{
    public class TestClock : IClock
    {
        private DateTime? _currentTime;

        public DateTime GetCurrentTime()
        {
            return _currentTime ?? DateTime.UtcNow;
        }

        public void SetCurrentTime(DateTime? mockTime)
        {
            _currentTime = mockTime;
        }

        public DateTime SetMockCurrentTime()
        {
            var time = new DateTime(3000, 1, 1, 1, 1, 1, DateTimeKind.Utc);
            _currentTime = time;
            return time;
        }

        public DateTime ForwardCurrentTime()
        {
            return ForwardCurrentTime(TimeSpan.FromDays(1));
        }

        public DateTime ForwardCurrentTime(TimeSpan timeSpan)
        {
            if (_currentTime == null)
                return SetMockCurrentTime();
            _currentTime = _currentTime.Value + timeSpan;
            return _currentTime.Value;
        }
    }
}
