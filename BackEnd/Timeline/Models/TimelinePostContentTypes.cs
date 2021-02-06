using System.Collections.Generic;

namespace Timeline.Models
{
    public static class TimelinePostContentTypes
    {
        public static string[] AllTypes { get; } = new string[] { Text, Image };
        public const string Text = "text";
        public const string Image = "image";
    }
}
