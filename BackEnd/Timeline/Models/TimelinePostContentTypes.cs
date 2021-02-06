using System.Collections.Generic;

namespace Timeline.Models
{
    public static class TimelinePostContentTypes
    {
#pragma warning disable CA1819 // Properties should not return arrays
        public static string[] AllTypes { get; } = new string[] { Text, Image };
#pragma warning restore CA1819 // Properties should not return arrays

        public const string Text = "text";
        public const string Image = "image";
    }
}
