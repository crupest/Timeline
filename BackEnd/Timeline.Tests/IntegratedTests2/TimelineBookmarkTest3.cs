using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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
        public async Task DeleteNotExist()
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
        }

        [Fact]
        public async Task DeleteNotLogin()
        {
            using var client = CreateDefaultClient();
            await client.TestJsonSendAsync(HttpMethod.Post, "v2/users/user/bookmarks/delete", new HttpTimelinebookmarkDeleteRequest
            {
                TimelineOwner = "user",
                TimelineName = "hello"
            }, expectedStatusCode: HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteForbid()
        {
            await CreateUserAsync("user2", "user2pw");
            using var client = CreateClientWithToken(await CreateTokenWithCredentialAsync("user2", "user2pw"));
            await client.TestJsonSendAsync(HttpMethod.Post, "v2/users/user/bookmarks/delete", new HttpTimelinebookmarkDeleteRequest
            {
                TimelineOwner = "user",
                TimelineName = "hello"
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
    }
}

