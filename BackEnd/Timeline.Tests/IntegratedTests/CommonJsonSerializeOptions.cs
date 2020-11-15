using System.Text.Json;
using System.Text.Json.Serialization;
using Timeline.Models.Converters;

namespace Timeline.Tests.IntegratedTests
{
    public static class CommonJsonSerializeOptions
    {
        public static JsonSerializerOptions Options { get; }

        static CommonJsonSerializeOptions()
        {
            Options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            Options.Converters.Add(new JsonStringEnumConverter());
            Options.Converters.Add(new JsonDateTimeConverter());
        }
    }
}
