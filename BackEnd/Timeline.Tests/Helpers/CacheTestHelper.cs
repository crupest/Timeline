using FluentAssertions;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Timeline.Models.Http;

namespace Timeline.Tests.Helpers
{
    public static class CacheTestHelper
    {
        public static async Task TestCache(HttpClient client, string getUrl)
        {
            EntityTagHeaderValue eTag;
            {
                var res = await client.GetAsync(getUrl);
                res.Should().HaveStatusCode(200);
                var cacheControlHeader = res.Headers.CacheControl;
                cacheControlHeader.Should().NotBeNull();
                cacheControlHeader!.NoCache.Should().BeTrue();
                cacheControlHeader.NoStore.Should().BeFalse();
                cacheControlHeader.Private.Should().BeTrue();
                cacheControlHeader.Public.Should().BeFalse();
                cacheControlHeader.MustRevalidate.Should().BeTrue();
                cacheControlHeader.MaxAge.Should().NotBeNull().And.Be(TimeSpan.FromDays(14));
                res.Headers.ETag.Should().NotBeNull();
                eTag = res.Headers.ETag!;
            }

            {
                using var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(client.BaseAddress!, getUrl),
                    Method = HttpMethod.Get,
                };
                request.Headers.TryAddWithoutValidation("If-None-Match", "\"dsdfd");
                var res = await client.SendAsync(request);
                await res.Should().HaveStatusCode(HttpStatusCode.BadRequest)
                    .And.HaveCommonBodyWithCodeAsync(ErrorCodes.Common.Header.IfNonMatch_BadFormat);
            }

            {
                using var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(client.BaseAddress!, getUrl),
                    Method = HttpMethod.Get,
                };
                request.Headers.TryAddWithoutValidation("If-None-Match", "\"aaa\"");
                var res = await client.SendAsync(request);
                res.Should().HaveStatusCode(HttpStatusCode.OK);
            }

            {
                using var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(client.BaseAddress!, getUrl),
                    Method = HttpMethod.Get,
                };
                request.Headers.Add("If-None-Match", eTag.ToString());
                var res = await client.SendAsync(request);
                res.Should().HaveStatusCode(HttpStatusCode.NotModified);
            }
        }
    }
}
