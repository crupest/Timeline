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
    public class TimelineBookmarkTest3 : IntegratedTestBase
    {
        public TimelineBookmarkTest3(ITestOutputHelper testOutput) : base(testOutput)
        {
        }

        protected override async Task OnInitializeAsync()
        {
            using var client = CreateClientAsUser();
            await client.TestJsonSendAsync(HttpMethod.Post, "v2/timelines", new HttpTimelineCreateRequest
            {
                Name = "hello"
            }, expectedStatusCode: HttpStatusCode.Created);

            await client.TestJsonSendAsync(HttpMethod.Post, "v2/timelines", new HttpTimelineCreateRequest
            {
                Name = "hello2"
            }, expectedStatusCode: HttpStatusCode.Created);

            await client.TestJsonSendAsync(HttpMethod.Post, "v2/users/user/bookmarks", new HttpTimelineBookmarkCreateRequest
            {
                TimelineOwner = "user",
                TimelineName = "hello"
            }, expectedStatusCode: HttpStatusCode.Created);

            await client.TestJsonSendAsync(HttpMethod.Post, "v2/users/user/bookmarks", new HttpTimelineBookmarkCreateRequest
            {
                TimelineOwner = "user",
                TimelineName = "hello2"
            }, expectedStatusCode: HttpStatusCode.Created);
        }

        [Fact]
        public async Task DeleteTest()
        {
            using var client = CreateClientAsUser();
            await client.TestJsonSendAsync(HttpMethod.Post, "v2/users/user/bookmarks/delete", new HttpTimelinebookmarkDeleteRequest
            {
                TimelineOwner = "user",
                TimelineName = "hello"
            }, expectedStatusCode: HttpStatusCode.NoContent);

            await client.TestJsonSendAsync(HttpMethod.Post, "v2/users/user/bookmarks/delete", new HttpTimelinebookmarkDeleteRequest
            {
                TimelineOwner = "user",
                TimelineName = "hello"
            }, expectedStatusCode: HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task MoveTest()
        {
            using var client = CreateClientAsUser();
            var a = await client.TestJsonSendAsync<TimelineBookmark>(HttpMethod.Post, "v2/users/user/bookmarks/move", new HttpTimelineBookmarkMoveRequest
            {
                TimelineOwner = "user",
                TimelineName = "hello",
                Position = 2
            });
            a.Position.Should().Be(2);

            var b = await client.TestJsonSendAsync<TimelineBookmark>(HttpMethod.Get, "v2/users/user/bookmarks/2");
            b.TimelineOwner.Should().Be("user");
            b.TimelineName.Should().Be("hello");

            await client.TestJsonSendAsync<TimelineBookmark>(HttpMethod.Post, "v2/users/user/bookmarks/move", new HttpTimelineBookmarkMoveRequest
            {
                TimelineOwner = "user",
                TimelineName = "hello",
            }, expectedStatusCode: HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task DeleteMoveNotExist()
        {
            using var client = CreateClientAsUser();

            await client.TestJsonSendAsync(HttpMethod.Post, "v2/users/user/bookmarks/delete", new HttpTimelinebookmarkDeleteRequest
            {
                TimelineOwner = "notexist",
                TimelineName = "hello"
            }, expectedStatusCode: HttpStatusCode.UnprocessableEntity);

            await client.TestJsonSendAsync(HttpMethod.Post, "v2/users/user/bookmarks/delete", new HttpTimelinebookmarkDeleteRequest
            {
                TimelineOwner = "user",
                TimelineName = "notexist"
            }, expectedStatusCode: HttpStatusCode.UnprocessableEntity);

            await client.TestJsonSendAsync(HttpMethod.Post, "v2/users/user/bookmarks/move", new HttpTimelineBookmarkMoveRequest
            {
                TimelineOwner = "notexist",
                TimelineName = "hello",
                Position = 2

            }, expectedStatusCode: HttpStatusCode.UnprocessableEntity);

            await client.TestJsonSendAsync(HttpMethod.Post, "v2/users/user/bookmarks/move", new HttpTimelineBookmarkMoveRequest
            {
                TimelineOwner = "user",
                TimelineName = "notexist",
                Position = 2
            }, expectedStatusCode: HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task DeleteMoveNotLogin()
        {
            using var client = CreateDefaultClient();
            await client.TestJsonSendAsync(HttpMethod.Post, "v2/users/user/bookmarks/delete", new HttpTimelinebookmarkDeleteRequest
            {
                TimelineOwner = "user",
                TimelineName = "hello"
            }, expectedStatusCode: HttpStatusCode.Unauthorized);

            await client.TestJsonSendAsync(HttpMethod.Post, "v2/users/user/bookmarks/move", new HttpTimelineBookmarkMoveRequest
            {
                TimelineOwner = "user",
                TimelineName = "hello",
                Position = 2
            }, expectedStatusCode: HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteMoveForbid()
        {
            await CreateUserAsync("user2", "user2pw");
            using var client = CreateClientWithToken(await CreateTokenWithCredentialAsync("user2", "user2pw"));
            await client.TestJsonSendAsync(HttpMethod.Post, "v2/users/user/bookmarks/delete", new HttpTimelinebookmarkDeleteRequest
            {
                TimelineOwner = "user",
                TimelineName = "hello"
            }, expectedStatusCode: HttpStatusCode.Forbidden);

            await client.TestJsonSendAsync(HttpMethod.Post, "v2/users/user/bookmarks/move", new HttpTimelineBookmarkMoveRequest
            {
                TimelineOwner = "user",
                TimelineName = "hello",
                Position = 2
            }, expectedStatusCode: HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteAdmin()
        {
            using var client = CreateClientAsAdmin();
            await client.TestJsonSendAsync(HttpMethod.Post, "v2/users/user/bookmarks/delete", new HttpTimelinebookmarkDeleteRequest
            {
                TimelineOwner = "user",
                TimelineName = "hello"
            }, expectedStatusCode: HttpStatusCode.NoContent);

            await client.TestJsonSendAsync(HttpMethod.Post, "v2/users/notexist/bookmarks/delete", new HttpTimelinebookmarkDeleteRequest
            {
                TimelineOwner = "user",
                TimelineName = "hello"
            }, expectedStatusCode: HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task MoveAdmin()
        {
            using var client = CreateClientAsAdmin();
            await client.TestJsonSendAsync(HttpMethod.Post, "v2/users/user/bookmarks/move", new HttpTimelineBookmarkMoveRequest
            {
                TimelineOwner = "user",
                TimelineName = "hello",
                Position = 2
            }, expectedStatusCode: HttpStatusCode.OK);

            await client.TestJsonSendAsync(HttpMethod.Post, "v2/users/notexist/bookmarks/move", new HttpTimelineBookmarkMoveRequest
            {
                TimelineOwner = "user",
                TimelineName = "hello",
                Position = 2
            }, expectedStatusCode: HttpStatusCode.NotFound);

        }
    }
}

