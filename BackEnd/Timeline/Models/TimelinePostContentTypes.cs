using System.Collections.Generic;

namespace Timeline.Models
{
    public static class TimelinePostDataKind
    {
        public static IReadOnlyList<string> AllTypes { get; } = new List<string> { Text, Image, Markdown };

        public const string Text = "text";
        public const string Image = "image";
        public const string Markdown = "markdown";
    }
}
