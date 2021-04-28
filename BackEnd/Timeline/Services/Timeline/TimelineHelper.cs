using System;

namespace Timeline.Services.Timeline
{
    public static class TimelineHelper
    {
        public static string ExtractTimelineName(string name, out bool isPersonal)
        {
            if (name.StartsWith("@", StringComparison.OrdinalIgnoreCase))
            {
                isPersonal = true;
                return name[1..];
            }
            else
            {
                isPersonal = false;
                return name;
            }
        }
    }
}
