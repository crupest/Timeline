using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Timeline.Models;
using Timeline.Models.Http;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests.IntegratedTests2
{
    public class TimelineBookmarkTest : IntegratedTestBase
    {
        public TimelineBookmarkTest(ITestOutputHelper testOutput) : base(testOutput)
        {
        }

        protected override async Task OnInitializeAsync()
        {
            using var client = CreateClientAsUser();
            await client.TestJsonSendAsync(HttpMethod.Post, "v2/timelines", new HttpTimelineCreateRequest
            {
                Name = "hello"
            }, expectedStatusCode: HttpStatusCode.Created);

        }

        [Fact]
        public async Task CreateAndGet()
        {
            using var client = CreateClientAsUser();
            var a = await client.TestJsonSendAsync<TimelineBookmark>(HttpMethod.Post, "users/user/bookmarks", new HttpTimelineBookmarkCreateRequest
            {
                TimelineOwner = "user",
                TimelineName = "hello"
            }, expectedStatusCode: HttpStatusCode.Created);

            a.TimelineOwner.Should().Be("user");
            a.TimelineName.Should().Be("hello");
            a.Position.Should().Be(1);

            var b = await client.TestJsonSendAsync<TimelineBookmark>(HttpMethod.Get, "users/user/bookmarks/1");
            b.TimelineName.Should().Be("hello");
            b.TimelineOwner.Should().Be("user");
            b.Position.Should().Be(1);
        }
    }
}

