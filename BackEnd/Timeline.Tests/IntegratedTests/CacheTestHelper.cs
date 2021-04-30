using FluentAssertions;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Timeline.Tests.IntegratedTests
{
    public static class CacheTestHelper
    {
        public static async Task TestCache(HttpClient client, string getUrl)
        {
            EntityTagHeaderValue eTag;

            {
                var res = await client.GetAsync(getUrl);
                res.StatusCode.Should().Be(HttpStatusCode.OK);
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

            await client.TestSendAssertErrorAsync(HttpMethod.Get, getUrl,
                expectedStatusCode: HttpStatusCode.BadRequest,
                errorCode: ErrorCodes.Common.Header.IfNonMatch_BadFormat,
                headerSetup: static (headers, _) =>
                {
                    headers.TryAddWithoutValidation("If-None-Match", "\"dsdfd");
                });

            await client.TestSendAsync(HttpMethod.Get, getUrl,
                expectedStatusCode: HttpStatusCode.OK,
                headerSetup: static (headers, _) =>
                {
                    headers.TryAddWithoutValidation("If-None-Match", "\"aaa\"");
                });

            await client.TestSendAsync(HttpMethod.Get, getUrl,
                expectedStatusCode: HttpStatusCode.NotModified,
                headerSetup: (headers, _) =>
                {
                    headers.Add("If-None-Match", eTag.ToString());
                });
        }
    }
}
