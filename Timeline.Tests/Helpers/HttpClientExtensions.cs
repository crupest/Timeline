using Newtonsoft.Json;
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
            return client.PatchAsync(url, new StringContent(
                JsonConvert.SerializeObject(body), Encoding.UTF8, MediaTypeNames.Application.Json));
        }

        public static Task<HttpResponseMessage> PutByteArrayAsync(this HttpClient client, string url, byte[] body, string mimeType)
        {
            var content = new ByteArrayContent(body);
            content.Headers.ContentLength = body.Length;
            content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
            return client.PutAsync(url, content);
        }
    }
}
