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
        public async Task CreateGetList()
        {
            using var client = CreateClientAsUser();
            var a = await client.TestJsonSendAsync<TimelineBookmark>(HttpMethod.Post, "v2/users/user/bookmarks", new HttpTimelineBookmarkCreateRequest
            {
                TimelineOwner = "user",
                TimelineName = "hello"
            }, expectedStatusCode: HttpStatusCode.Created);

            a.TimelineOwner.Should().Be("user");
            a.TimelineName.Should().Be("hello");
            a.Position.Should().Be(1);

            var b = await client.TestJsonSendAsync<TimelineBookmark>(HttpMethod.Get, "v2/users/user/bookmarks/1");
            b.TimelineName.Should().Be("hello");
            b.TimelineOwner.Should().Be("user");
            b.Position.Should().Be(1);

            var c = await client.TestJsonSendAsync<Page<TimelineBookmark>>(HttpMethod.Get, "v2/users/user/bookmarks");
            c.TotalCount.Should().Be(1);
            c.Items.Should().ContainSingle();
            c.Items[0].TimelineOwner.Should().Be("user");
            c.Items[0].TimelineName.Should().Be("hello");
            c.Items[0].Position.Should().Be(1);
        }

        [Fact]
        public async Task CreateUserNotExist()
        {
            using var client = CreateClientAsUser();
            await client.TestJsonSendAsync(HttpMethod.Post, "v2/users/notexist/bookmarks", new HttpTimelineBookmarkCreateRequest
            {
                TimelineOwner = "user",
                TimelineName = "hello"
            }, expectedStatusCode: HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateTimelineNotExist()
        {
            using var client = CreateClientAsUser();
            await client.TestJsonSendAsync(HttpMethod.Post, "v2/users/user/bookmarks", new HttpTimelineBookmarkCreateRequest
            {
                TimelineOwner = "user",
                TimelineName = "notexist"
            }, expectedStatusCode: HttpStatusCode.UnprocessableEntity);

            await client.TestJsonSendAsync(HttpMethod.Post, "v2/users/user/bookmarks", new HttpTimelineBookmarkCreateRequest
            {
                TimelineOwner = "notexist",
                TimelineName = "notexist"
            }, expectedStatusCode: HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task CreateAlreadyExist()
        {
            using var client = CreateClientAsUser();

            await client.TestJsonSendAsync<TimelineBookmark>(HttpMethod.Post, "v2/users/user/bookmarks", new HttpTimelineBookmarkCreateRequest
            {
                TimelineOwner = "user",
                TimelineName = "hello"
            }, expectedStatusCode: HttpStatusCode.Created);

            await client.TestJsonSendAsync(HttpMethod.Post, "v2/users/user/bookmarks", new HttpTimelineBookmarkCreateRequest
            {
                TimelineOwner = "user",
                TimelineName = "hello"
            }, expectedStatusCode: HttpStatusCode.UnprocessableEntity);
        }
    }
}

