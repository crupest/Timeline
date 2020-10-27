using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Timeline.Tests.Helpers
{
    public static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> PatchAsJsonAsync<T>(this HttpClient client, string url, T body)
        {
            return client.PatchAsJsonAsync(new Uri(url, UriKind.RelativeOrAbsolute), body);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
        public static Task<HttpResponseMessage> PatchAsJsonAsync<T>(this HttpClient client, Uri url, T body)
        {
            return client.PatchAsync(url, new StringContent(
                JsonConvert.SerializeObject(body), Encoding.UTF8, MediaTypeNames.Application.Json));
        }

        public static Task<HttpResponseMessage> PutByteArrayAsync(this HttpClient client, string url, byte[] body, string mimeType)
        {
            return client.PutByteArrayAsync(new Uri(url, UriKind.RelativeOrAbsolute), body, mimeType);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
        public static Task<HttpResponseMessage> PutByteArrayAsync(this HttpClient client, Uri url, byte[] body, string mimeType)
        {
            var content = new ByteArrayContent(body);
            content.Headers.ContentLength = body.Length;
            content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
            return client.PutAsync(url, content);
        }

        public static Task<HttpResponseMessage> PutStringAsync(this HttpClient client, string url, string body, string mimeType = null)
        {
            return client.PutStringAsync(new Uri(url, UriKind.RelativeOrAbsolute), body, mimeType);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
        public static Task<HttpResponseMessage> PutStringAsync(this HttpClient client, Uri url, string body, string mimeType = null)
        {
            var content = new StringContent(body, Encoding.UTF8, mimeType ?? MediaTypeNames.Text.Plain);
            return client.PutAsync(url, content);
        }
    }
}
