using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Timeline.Models.Converters;
using Timeline.Models.Http;

namespace Timeline.Tests.Helpers
{
    public static class HttpResponseExtensions
    {
        public static JsonSerializerOptions JsonSerializerOptions { get; }

        static HttpResponseExtensions()
        {
            JsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            JsonSerializerOptions.Converters.Add(new JsonDateTimeConverter());
        }

        public static async Task<T?> ReadBodyAsJsonAsync<T>(this HttpResponseMessage response)
        {
            var stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonSerializerOptions);
        }
    }
}
