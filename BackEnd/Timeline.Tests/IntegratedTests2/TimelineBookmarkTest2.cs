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
    public class TimelineBookmarkTest2 : IntegratedTestBase
    {
        public TimelineBookmarkTest2(ITestOutputHelper testOutput) : base(testOutput)
        {
        }

        protected override async Task OnInitializeAsync()
        {
            using var client = CreateClientAsUser();
            await client.TestJsonSendAsync(HttpMethod.Post, "v2/timelines", new HttpTimelineCreateRequest
            {
                Name = "hello"
            }, expectedStatusCode: HttpStatusCode.Created);

            await client.TestJsonSendAsync(HttpMethod.Post, "v2/users/user/bookmarks", new HttpTimelineBookmarkCreateRequest
            {
                TimelineOwner = "user",
                TimelineName = "hello"
            }, expectedStatusCode: HttpStatusCode.Created);
        }

        private async Task ChangeVisibilityAsync(TimelineVisibility visibility)
        {
            using var client = CreateClientAsUser();
            await client.TestJsonSendAsync(HttpMethod.Put, "v2/users/user/bookmarks/visibility", new HttpTimelineBookmarkVisibility { Visibility = visibility }, expectedStatusCode: HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task ChangeVisibilityShouldWork()
        {
            using var client = CreateClientAsUser();
            var a = await client.TestJsonSendAsync<HttpTimelineBookmarkVisibility>(HttpMethod.Get, "v2/users/user/bookmarks/visibility", expectedStatusCode: HttpStatusCode.OK);
            a.Visibility.Should().Be(TimelineVisibility.Private);

            await client.TestJsonSendAsync(HttpMethod.Put, "v2/users/user/bookmarks/visibility", new HttpTimelineBookmarkVisibility { Visibility = TimelineVisibility.Register }, expectedStatusCode: HttpStatusCode.NoContent);
            var b = await client.TestJsonSendAsync<HttpTimelineBookmarkVisibility>(HttpMethod.Get, "v2/users/user/bookmarks/visibility", expectedStatusCode: HttpStatusCode.OK);
            b.Visibility.Should().Be(TimelineVisibility.Register);

            await client.TestJsonSendAsync(HttpMethod.Put, "v2/users/user/bookmarks/visibility", new HttpTimelineBookmarkVisibility { Visibility = TimelineVisibility.Public }, expectedStatusCode: HttpStatusCode.NoContent);
            var c = await client.TestJsonSendAsync<HttpTimelineBookmarkVisibility>(HttpMethod.Get, "v2/users/user/bookmarks/visibility", expectedStatusCode: HttpStatusCode.OK);
            c.Visibility.Should().Be(TimelineVisibility.Public);
        }

        [Fact]
        public async Task AnonymousCantSeePrivate()
        {
            using var client = CreateDefaultClient();
            await client.TestJsonSendAsync(HttpMethod.Get, "v2/users/user/bookmarks", expectedStatusCode: HttpStatusCode.Forbidden);
            await client.TestJsonSendAsync(HttpMethod.Get, "v2/users/user/bookmarks/1", expectedStatusCode: HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task OtherUserCantSeePrivate()
        {
            await CreateUserAsync("user2", "user2pw");
            using var client = CreateClientWithToken(await CreateTokenWithCredentialAsync("user2", "user2pw"));
            await client.TestJsonSendAsync(HttpMethod.Get, "v2/users/user/bookmarks", expectedStatusCode: HttpStatusCode.Forbidden);
            await client.TestJsonSendAsync(HttpMethod.Get, "v2/users/user/bookmarks/1", expectedStatusCode: HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task AdminCanSeePrivate()
        {
            using var client = CreateClientAsAdmin();
            await client.TestJsonSendAsync(HttpMethod.Get, "v2/users/user/bookmarks", expectedStatusCode: HttpStatusCode.OK);
            await client.TestJsonSendAsync(HttpMethod.Get, "v2/users/user/bookmarks/1", expectedStatusCode: HttpStatusCode.OK);
        }

        [Fact]
        public async Task AnonymousCantSeeRegister()
        {
            await ChangeVisibilityAsync(TimelineVisibility.Register);
            using var client = CreateDefaultClient();
            await client.TestJsonSendAsync(HttpMethod.Get, "v2/users/user/bookmarks", expectedStatusCode: HttpStatusCode.Forbidden);
            await client.TestJsonSendAsync(HttpMethod.Get, "v2/users/user/bookmarks/1", expectedStatusCode: HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task OtherUserCanSeeRegister()
        {
            await ChangeVisibilityAsync(TimelineVisibility.Register);
            await CreateUserAsync("user2", "user2pw");
            using var client = CreateClientWithToken(await CreateTokenWithCredentialAsync("user2", "user2pw"));
            await client.TestJsonSendAsync(HttpMethod.Get, "v2/users/user/bookmarks", expectedStatusCode: HttpStatusCode.OK);
            await client.TestJsonSendAsync(HttpMethod.Get, "v2/users/user/bookmarks/1", expectedStatusCode: HttpStatusCode.OK);
        }

        [Fact]
        public async Task AdminCanSeeRegister()
        {
            await ChangeVisibilityAsync(TimelineVisibility.Register);
            using var client = CreateClientAsAdmin();
            await client.TestJsonSendAsync(HttpMethod.Get, "v2/users/user/bookmarks", expectedStatusCode: HttpStatusCode.OK);
            await client.TestJsonSendAsync(HttpMethod.Get, "v2/users/user/bookmarks/1", expectedStatusCode: HttpStatusCode.OK);
        }

        [Fact]
        public async Task AnonymousCanSeePublic()
        {
            await ChangeVisibilityAsync(TimelineVisibility.Public);
            using var client = CreateDefaultClient();
            await client.TestJsonSendAsync(HttpMethod.Get, "v2/users/user/bookmarks", expectedStatusCode: HttpStatusCode.OK);
            await client.TestJsonSendAsync(HttpMethod.Get, "v2/users/user/bookmarks/1", expectedStatusCode: HttpStatusCode.OK);
        }

        [Fact]
        public async Task OtherUserCanSeePublic()
        {
            await ChangeVisibilityAsync(TimelineVisibility.Public);
            await CreateUserAsync("user2", "user2pw");
            using var client = CreateClientWithToken(await CreateTokenWithCredentialAsync("user2", "user2pw"));
            await client.TestJsonSendAsync(HttpMethod.Get, "v2/users/user/bookmarks", expectedStatusCode: HttpStatusCode.OK);
            await client.TestJsonSendAsync(HttpMethod.Get, "v2/users/user/bookmarks/1", expectedStatusCode: HttpStatusCode.OK);
        }

        [Fact]
        public async Task AdminCanSeePublic()
        {
            await ChangeVisibilityAsync(TimelineVisibility.Public);
            using var client = CreateClientAsAdmin();
            await client.TestJsonSendAsync(HttpMethod.Get, "v2/users/user/bookmarks", expectedStatusCode: HttpStatusCode.OK);
            await client.TestJsonSendAsync(HttpMethod.Get, "v2/users/user/bookmarks/1", expectedStatusCode: HttpStatusCode.OK);
        }
    }
}

