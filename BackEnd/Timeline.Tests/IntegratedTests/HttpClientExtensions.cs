using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Timeline.Tests.IntegratedTests
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PatchAsJsonAsync<T>(this HttpClient client, string url, T body)
        {
            using var reqContent = JsonContent.Create(body, options: CommonJsonSerializeOptions.Options);
            return await client.PatchAsync(url, reqContent);
        }

        public static Task<HttpResponseMessage> PutAsync(this HttpClient client, string url)
        {
            return client.PutAsync(url, null!);
        }

        public static Task<HttpResponseMessage> PutByteArrayAsync(this HttpClient client, string url, byte[] body, string mimeType)
        {
            return client.PutByteArrayAsync(new Uri(url, UriKind.RelativeOrAbsolute), body, mimeType);
        }

        public static async Task<HttpResponseMessage> PutByteArrayAsync(this HttpClient client, Uri url, byte[] body, string mimeType)
        {
            using var content = new ByteArrayContent(body);
            content.Headers.ContentLength = body.Length;
            content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
            return await client.PutAsync(url, content);
        }

        public static Task<HttpResponseMessage> PutStringAsync(this HttpClient client, string url, string body, string? mimeType = null)
        {
            return client.PutStringAsync(new Uri(url, UriKind.RelativeOrAbsolute), body, mimeType);
        }

        public static async Task<HttpResponseMessage> PutStringAsync(this HttpClient client, Uri url, string body, string? mimeType = null)
        {
            using var content = new StringContent(body, Encoding.UTF8, mimeType ?? MediaTypeNames.Text.Plain);
            return await client.PutAsync(url, content);
        }
    }
}
