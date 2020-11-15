using FluentAssertions;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Timeline.Models.Http;

namespace Timeline.Tests.IntegratedTests
{
    public delegate void HeaderSetup(HttpRequestHeaders requestHeaders, HttpContentHeaders? contentHeaders);

    public static class HttpClientTestExtensions
    {
        public static async Task<HttpResponseMessage> TestSendAsync(this HttpClient client, HttpMethod method, string url, HttpContent? body = null, HttpStatusCode expectedStatusCode = HttpStatusCode.OK, HeaderSetup? headerSetup = null)
        {
            using var req = new HttpRequestMessage
            {
                Method = method,
                RequestUri = new Uri(url, UriKind.Relative),
                Content = body
            };
            headerSetup?.Invoke(req.Headers, body?.Headers);
            var res = await client.SendAsync(req);
            res.StatusCode.Should().Be(expectedStatusCode);
            return res;
        }

        public static async Task<T> AssertJsonBodyAsync<T>(HttpResponseMessage response)
        {
            var body = await response.Content.ReadFromJsonAsync<T>(CommonJsonSerializeOptions.Options);
            body.Should().NotBeNull($"Body is not json format of type {typeof(T).FullName}");
            return body!;
        }

        public static async Task TestJsonSendAsync(this HttpClient client, HttpMethod method, string url, object? jsonBody = null, HttpStatusCode expectedStatusCode = HttpStatusCode.OK, HeaderSetup? headerSetup = null)
        {
            using JsonContent? reqContent = jsonBody == null ? null : JsonContent.Create(jsonBody, options: CommonJsonSerializeOptions.Options);
            await client.TestSendAsync(method, url, reqContent, expectedStatusCode, headerSetup);
        }

        public static async Task<T> TestJsonSendAsync<T>(this HttpClient client, HttpMethod method, string url, object? jsonBody = null, HttpStatusCode expectedStatusCode = HttpStatusCode.OK, HeaderSetup? headerSetup = null)
        {
            using JsonContent? reqContent = jsonBody == null ? null : JsonContent.Create(jsonBody, options: CommonJsonSerializeOptions.Options);
            var res = await client.TestSendAsync(method, url, reqContent, expectedStatusCode, headerSetup);
            var resBody = await AssertJsonBodyAsync<T>(res);
            return resBody;
        }

        public static async Task TestGetAsync(this HttpClient client, string url, HttpStatusCode expectedStatusCode = HttpStatusCode.OK, HeaderSetup? headerSetup = null)
        {
            await client.TestJsonSendAsync(HttpMethod.Get, url, expectedStatusCode: expectedStatusCode, headerSetup: headerSetup);
        }

        public static async Task<T> TestGetAsync<T>(this HttpClient client, string url, HttpStatusCode expectedStatusCode = HttpStatusCode.OK, HeaderSetup? headerSetup = null)
        {
            return await client.TestJsonSendAsync<T>(HttpMethod.Get, url, expectedStatusCode: expectedStatusCode, headerSetup: headerSetup);
        }

        public static async Task TestPostAsync(this HttpClient client, string url, object? jsonBody = null, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            await client.TestJsonSendAsync(HttpMethod.Post, url, jsonBody, expectedStatusCode: expectedStatusCode);
        }

        public static async Task<T> TestPostAsync<T>(this HttpClient client, string url, object? jsonBody = null, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            return await client.TestJsonSendAsync<T>(HttpMethod.Post, url, jsonBody, expectedStatusCode: expectedStatusCode);
        }

        public static async Task TestPutAsync(this HttpClient client, string url, object? jsonBody = null, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            await client.TestJsonSendAsync(HttpMethod.Put, url, jsonBody, expectedStatusCode: expectedStatusCode);
        }

        public static async Task<T> TestPutAsync<T>(this HttpClient client, string url, object? jsonBody = null, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            return await client.TestJsonSendAsync<T>(HttpMethod.Put, url, jsonBody, expectedStatusCode: expectedStatusCode);
        }

        public static async Task<T> TestPatchAsync<T>(this HttpClient client, string url, object? jsonBody = null, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            return await client.TestJsonSendAsync<T>(HttpMethod.Patch, url, jsonBody, expectedStatusCode: expectedStatusCode);
        }

        public static async Task TestDeleteAsync(this HttpClient client, string url, bool? delete = null, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var body = await client.TestJsonSendAsync<CommonDeleteResponse>(HttpMethod.Delete, url, expectedStatusCode: expectedStatusCode);
            if (delete.HasValue)
                body.Data.Delete.Should().Be(delete.Value);
        }

        public static async Task TestSendAssertErrorAsync(this HttpClient client, HttpMethod method, string url, HttpContent? body = null, HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest, int? errorCode = null, HeaderSetup? headerSetup = null)
        {
            var res = await client.TestSendAsync(method, url, body, expectedStatusCode, headerSetup);
            if (errorCode.HasValue)
            {
                var resBody = await AssertJsonBodyAsync<CommonResponse>(res);
                resBody.Code.Should().Be(errorCode.Value);
            }
        }

        public static async Task TestJsonSendAssertErrorAsync(this HttpClient client, HttpMethod method, string url, object? jsonBody = null, HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest, int? errorCode = null, HeaderSetup? headerSetup = null)
        {
            using JsonContent? reqContent = jsonBody == null ? null : JsonContent.Create(jsonBody, options: CommonJsonSerializeOptions.Options);
            await client.TestSendAssertErrorAsync(method, url, reqContent, expectedStatusCode, errorCode, headerSetup);
        }

        public static async Task TestGetAssertErrorAsync(this HttpClient client, string url, object? jsonBody = null, HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest, int? errorCode = null, HeaderSetup? headerSetup = null)
        {
            await client.TestJsonSendAssertErrorAsync(HttpMethod.Get, url, jsonBody, expectedStatusCode, errorCode, headerSetup);
        }

        public static async Task TestPostAssertErrorAsync(this HttpClient client, string url, object? jsonBody = null, HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest, int? errorCode = null, HeaderSetup? headerSetup = null)
        {
            await client.TestJsonSendAssertErrorAsync(HttpMethod.Post, url, jsonBody, expectedStatusCode, errorCode, headerSetup);
        }

        public static async Task TestPutAssertErrorAsync(this HttpClient client, string url, object? jsonBody = null, HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest, int? errorCode = null, HeaderSetup? headerSetup = null)
        {
            await client.TestJsonSendAssertErrorAsync(HttpMethod.Put, url, jsonBody, expectedStatusCode, errorCode, headerSetup);
        }

        public static async Task TestDeleteAssertErrorAsync(this HttpClient client, string url, object? jsonBody = null, HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest, int? errorCode = null, HeaderSetup? headerSetup = null)
        {
            await client.TestJsonSendAssertErrorAsync(HttpMethod.Delete, url, jsonBody, expectedStatusCode, errorCode, headerSetup);
        }

        public static async Task TestSendAssertInvalidModelAsync(this HttpClient client, HttpMethod method, string url, HttpContent? body = null)
        {
            await client.TestSendAssertErrorAsync(method, url, body, expectedStatusCode: HttpStatusCode.BadRequest, errorCode: ErrorCodes.Common.InvalidModel);
        }

        public static async Task TestJsonSendAssertInvalidModelAsync(this HttpClient client, HttpMethod method, string url, object? jsonBody = null)
        {
            await client.TestJsonSendAssertErrorAsync(method, url, jsonBody, expectedStatusCode: HttpStatusCode.BadRequest, errorCode: ErrorCodes.Common.InvalidModel);
        }

        public static async Task TestGetAssertInvalidModelAsync(this HttpClient client, string url)
        {
            await client.TestJsonSendAssertInvalidModelAsync(HttpMethod.Get, url);
        }

        public static async Task TestPostAssertInvalidModelAsync(this HttpClient client, string url, object? jsonBody = null)
        {
            await client.TestJsonSendAssertInvalidModelAsync(HttpMethod.Post, url, jsonBody);
        }

        public static async Task TestDeleteAssertInvalidModelAsync(this HttpClient client, string url, object? jsonBody = null)
        {
            await client.TestJsonSendAssertInvalidModelAsync(HttpMethod.Delete, url, jsonBody);
        }

        public static async Task TestJsonSendAssertUnauthorizedAsync(this HttpClient client, HttpMethod method, string url, object? jsonBody = null, int? errorCode = null, HeaderSetup? headerSetup = null)
        {
            await client.TestJsonSendAssertErrorAsync(method, url, jsonBody, HttpStatusCode.Unauthorized, errorCode, headerSetup);
        }

        public static async Task TestGetAssertUnauthorizedAsync(this HttpClient client, string url, object? jsonBody = null, int? errorCode = null, HeaderSetup? headerSetup = null)
        {
            await client.TestJsonSendAssertUnauthorizedAsync(HttpMethod.Get, url, jsonBody, errorCode, headerSetup);
        }

        public static async Task TestPostAssertUnauthorizedAsync(this HttpClient client, string url, object? jsonBody = null, int? errorCode = null, HeaderSetup? headerSetup = null)
        {
            await client.TestJsonSendAssertUnauthorizedAsync(HttpMethod.Post, url, jsonBody, errorCode, headerSetup);
        }

        public static async Task TestDeleteAssertUnauthorizedAsync(this HttpClient client, string url, object? jsonBody = null, int? errorCode = null, HeaderSetup? headerSetup = null)
        {
            await client.TestJsonSendAssertUnauthorizedAsync(HttpMethod.Delete, url, jsonBody, errorCode, headerSetup);
        }

        public static async Task TestJsonSendAssertForbiddenAsync(this HttpClient client, HttpMethod method, string url, object? jsonBody = null, int? errorCode = null, HeaderSetup? headerSetup = null)
        {
            await client.TestJsonSendAssertErrorAsync(method, url, jsonBody, HttpStatusCode.Forbidden, errorCode, headerSetup);
        }

        public static async Task TestGetAssertForbiddenAsync(this HttpClient client, string url, object? jsonBody = null, int? errorCode = null, HeaderSetup? headerSetup = null)
        {
            await client.TestJsonSendAssertForbiddenAsync(HttpMethod.Get, url, jsonBody, errorCode, headerSetup);
        }

        public static async Task TestPostAssertForbiddenAsync(this HttpClient client, string url, object? jsonBody = null, int? errorCode = null, HeaderSetup? headerSetup = null)
        {
            await client.TestJsonSendAssertForbiddenAsync(HttpMethod.Post, url, jsonBody, errorCode, headerSetup);
        }

        public static async Task TestDeleteAssertForbiddenAsync(this HttpClient client, string url, object? jsonBody = null, int? errorCode = null, HeaderSetup? headerSetup = null)
        {
            await client.TestJsonSendAssertForbiddenAsync(HttpMethod.Delete, url, jsonBody, errorCode, headerSetup);
        }

        public static async Task TestJsonSendAssertNotFoundAsync(this HttpClient client, HttpMethod method, string url, object? jsonBody = null, int? errorCode = null, HeaderSetup? headerSetup = null)
        {
            await client.TestJsonSendAssertErrorAsync(method, url, jsonBody, HttpStatusCode.NotFound, errorCode, headerSetup);
        }

        public static async Task TestGetAssertNotFoundAsync(this HttpClient client, string url, object? jsonBody = null, int? errorCode = null, HeaderSetup? headerSetup = null)
        {
            await client.TestJsonSendAssertNotFoundAsync(HttpMethod.Get, url, jsonBody, errorCode, headerSetup);
        }

        public static async Task<HttpResponseMessage> TestPutByteArrayAsync(this HttpClient client, string url, byte[] body, string mimeType, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            using var content = new ByteArrayContent(body);
            content.Headers.ContentLength = body.Length;
            content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
            return await client.TestSendAsync(HttpMethod.Put, url, content, expectedStatusCode);
        }

        public static async Task TestPutByteArrayAssertErrorAsync(this HttpClient client, string url, byte[] body, string mimeType, HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest, int? errorCode = null)
        {
            var res = await client.TestPutByteArrayAsync(url, body, mimeType, expectedStatusCode);
            if (errorCode.HasValue)
            {
                var resBody = await AssertJsonBodyAsync<CommonResponse>(res);
                resBody.Code.Should().Be(errorCode.Value);
            }
        }

        public static async Task TestPutByteArrayAssertInvalidModelAsync(this HttpClient client, string url, byte[] body, string mimeType)
        {
            await client.TestPutByteArrayAssertErrorAsync(url, body, mimeType, errorCode: ErrorCodes.Common.InvalidModel);
        }
    }
}
