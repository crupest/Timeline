using System.Collections.Generic;
using System.Text;

namespace Timeline.Helpers
{
    public static class MyLogHelper
    {
        public static KeyValuePair<string, object> Pair(string key, object value) => new KeyValuePair<string, object>(key, value);

        public static string FormatLogMessage(string summary, params KeyValuePair<string, object>[] properties)
        {
            var builder = new StringBuilder();
            builder.Append(summary);
            foreach (var property in properties)
            {
                builder.AppendLine();
                builder.Append(property.Key);
                builder.Append(" : ");
                builder.Append(property.Value);
            }
            return builder.ToString();
        }
    }
}
