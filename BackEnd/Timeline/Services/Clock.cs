using System;

namespace Timeline.Services
{
    /// <summary>
    /// Convenient for unit test.
    /// </summary>
    public interface IClock
    {
        /// <summary>
        /// Get current time.
        /// </summary>
        /// <returns>Current time.</returns>
        DateTime GetCurrentTime();
    }

    public class Clock : IClock
    {
        public Clock()
        {

        }

        public DateTime GetCurrentTime()
        {
            return DateTime.UtcNow;
        }
    }
}
