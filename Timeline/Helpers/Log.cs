using System.Collections.Generic;
using System.Text;

namespace Timeline.Helpers
{
    public static class Log
    {
        public static string Format(string summary, params (string, object?)[] properties)
        {
            var builder = new StringBuilder();
            builder.Append(summary);
            foreach (var property in properties)
            {
                var (key, value) = property;
                builder.AppendLine();
                builder.Append(key);
                builder.Append(" : ");
                builder.Append(value);
            }
            return builder.ToString();
        }
    }
}
