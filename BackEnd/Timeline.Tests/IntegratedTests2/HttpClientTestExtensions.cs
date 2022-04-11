using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Timeline.Tests.Helpers;

namespace Timeline.Tests.IntegratedTests2
{
    public delegate Task RequestSetupAsync(HttpRequestMessage httpRequest);

    public static class HttpClientTestExtensions
    {
        public static async Task<HttpResponseMessage> TestSendAsync(this HttpClient client, HttpMethod method, string url, HttpContent? body = null, HttpStatusCode? expectedStatusCode = null, RequestSetupAsync? requestSetup = null)
        {
            using var req = new HttpRequestMessage
            {
                Method = method,
                RequestUri = new Uri(url, UriKind.Relative),
                Content = body
            };
            var task = requestSetup?.Invoke(req);
            if (task is not null) await task;
            var res = await client.SendAsync(req);
            if (expectedStatusCode is null)
            {
                ((int)res.StatusCode).Should().BeGreaterThanOrEqualTo(200).And.BeLessThan(300);
            }
            else
            {
                res.StatusCode.Should().Be(expectedStatusCode.Value);
            }
            return res;
        }

        public static async Task<T> AssertJsonBodyAsync<T>(HttpResponseMessage response)
        {
            var body = await response.Content.ReadFromJsonAsync<T>(CommonJsonSerializeOptions.Options);
            body.Should().NotBeNull($"Body is not json format of type {typeof(T).FullName}");
            return body!;
        }

        public static async Task TestJsonSendAsync(this HttpClient client, HttpMethod method, string url, object? jsonBody = null, HttpStatusCode? expectedStatusCode = null, RequestSetupAsync? requestSetup = null)
        {
            using JsonContent? reqContent = jsonBody is null ? null : JsonContent.Create(jsonBody, options: CommonJsonSerializeOptions.Options);
            await client.TestSendAsync(method, url, reqContent, expectedStatusCode, requestSetup);
        }

        public static async Task<T> TestJsonSendAsync<T>(this HttpClient client, HttpMethod method, string url, object? jsonBody = null, HttpStatusCode? expectedStatusCode = null, RequestSetupAsync? requestSetup = null)
        {
            using JsonContent? reqContent = jsonBody == null ? null : JsonContent.Create(jsonBody, options: CommonJsonSerializeOptions.Options);
            var res = await client.TestSendAsync(method, url, reqContent, expectedStatusCode, requestSetup);
            var resBody = await AssertJsonBodyAsync<T>(res);
            return resBody;
        }
    }
}
