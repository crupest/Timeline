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
    public class TimelinePatchAndDeleteTest : IntegratedTestBase
    {

        public TimelinePatchAndDeleteTest(ITestOutputHelper testOutput) : base(testOutput)
        {
        }

        protected override async Task OnInitializeAsync()
        {
            using var client = CreateClientAsUser();
            await client.TestJsonSendAsync<HttpTimeline>(HttpMethod.Post, "v2/timelines", new HttpTimelineCreateRequest
            {
                Name = "hello"
            }, expectedStatusCode: HttpStatusCode.Created);
        }

        [Fact]
        public async Task PatchTest()
        {
            using var client = CreateClientAsUser();
            var b = await client.TestJsonSendAsync<HttpTimeline>(HttpMethod.Patch, "v2/timelines/user/hello", new HttpTimelinePatchRequest
            {
                Name = "hello2",
                Title = "Hello",
                Description = "A Description.",
                Visibility = TimelineVisibility.Public,
                Color = "#FFFFFF"
            });

            b.Name.Should().Be("hello2");
            b.Title.Should().Be("Hello");
            b.Description.Should().Be("A Description.");
            b.Visibility.Should().Be(TimelineVisibility.Public);
            b.Color.Should().Be("#FFFFFF");
        }

        [Fact]
        public async Task PatchNotExist()
        {
            using var client = CreateClientAsUser();
            await client.TestJsonSendAsync(HttpMethod.Patch, "v2/timelines/user/notexist", new HttpTimelinePatchRequest(),
                expectedStatusCode: HttpStatusCode.NotFound);
            await client.TestJsonSendAsync(HttpMethod.Patch, "v2/timelines/notexist/notexist", new HttpTimelinePatchRequest(),
                expectedStatusCode: HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task PatchNotLogin()
        {
            using var client = CreateDefaultClient();
            await client.TestJsonSendAsync(HttpMethod.Patch, "v2/timelines/user/hello", new HttpTimelinePatchRequest(),
                expectedStatusCode: HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task PatchForbid()
        {
            await CreateUserAsync("user2", "user2pw");
            using var client = CreateClientWithToken(await CreateTokenWithCredentialAsync("user2", "user2pw"));
            await client.TestJsonSendAsync(HttpMethod.Patch, "v2/timelines/user/hello", new HttpTimelinePatchRequest(),
                expectedStatusCode: HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteTest()
        {
            using var client = CreateClientAsUser();
            await client.TestSendAsync(HttpMethod.Delete, "v2/timelines/user/hello", expectedStatusCode: HttpStatusCode.NoContent);
            await client.TestSendAsync(HttpMethod.Get, "v2/timelines/user/hello", expectedStatusCode: HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteNotExist()
        {
            using var client = CreateClientAsUser();
            await client.TestSendAsync(HttpMethod.Delete, "v2/timelines/user/notexist", expectedStatusCode: HttpStatusCode.NotFound);
            await client.TestSendAsync(HttpMethod.Delete, "v2/timelines/notexist/notexist", expectedStatusCode: HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteNotLogin()
        {
            using var client = CreateDefaultClient();
            await client.TestSendAsync(HttpMethod.Delete, "v2/timelines/user/hello", expectedStatusCode: HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteForbid()
        {
            await CreateUserAsync("user2", "user2pw");
            using var client = CreateClientWithToken(await CreateTokenWithCredentialAsync("user2", "user2pw"));
            await client.TestSendAsync(HttpMethod.Delete, "v2/timelines/user/hello",
                expectedStatusCode: HttpStatusCode.Forbidden);
        }
    }
}

