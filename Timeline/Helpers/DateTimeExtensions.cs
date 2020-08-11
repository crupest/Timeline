using System;

namespace Timeline.Helpers
{
    public static class DateTimeExtensions
    {
        public static DateTime MyToUtc(this DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Utc) return dateTime;
            if (dateTime.Kind == DateTimeKind.Local) return dateTime.ToUniversalTime();
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }
    }
}
