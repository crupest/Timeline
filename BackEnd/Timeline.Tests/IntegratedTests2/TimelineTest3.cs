using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Timeline.Models.Http;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests.IntegratedTests2
{
    public class TimelineTest3 : IntegratedTestBase
    {

        public TimelineTest3(ITestOutputHelper testOutput) : base(testOutput)
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
        public async Task MemberTest()
        {
            using var client = CreateClientAsUser();
            await client.TestSendAsync(HttpMethod.Put, "v2/timelines/user/hello/members/admin", expectedStatusCode: HttpStatusCode.NoContent);

            var t = await client.TestJsonSendAsync<HttpTimeline>(HttpMethod.Get, "v2/timelines/user/hello");
            t.Members.Should().ContainSingle().Which.Username.Should().Be("admin");

            await client.TestSendAsync(HttpMethod.Delete, "v2/timelines/user/hello/members/admin", expectedStatusCode: HttpStatusCode.NoContent);

            var b = await client.TestJsonSendAsync<HttpTimeline>(HttpMethod.Get, "v2/timelines/user/hello");
            b.Members.Should().BeEmpty();
        }

        [Fact]
        public async Task MemberPutNotExist()
        {
            using var client = CreateClientAsUser();
            await client.TestSendAsync(HttpMethod.Put, "v2/timelines/user/notexist/members/admin",
                expectedStatusCode: HttpStatusCode.NotFound);
            await client.TestSendAsync(HttpMethod.Put, "v2/timelines/notexist/notexist/members/admin",
                expectedStatusCode: HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task MemberDeleteNotExist()
        {
            using var client = CreateClientAsUser();
            await client.TestSendAsync(HttpMethod.Delete, "v2/timelines/user/notexist/members/admin",
                expectedStatusCode: HttpStatusCode.NotFound);
            await client.TestSendAsync(HttpMethod.Delete, "v2/timelines/notexist/notexist/members/admin",
                expectedStatusCode: HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task MemberModifyUserNotExist()
        {
            using var client = CreateClientAsUser();
            await client.TestSendAsync(HttpMethod.Put, "v2/timelines/user/hello/members/notexist",
                expectedStatusCode: HttpStatusCode.UnprocessableEntity);
            await client.TestSendAsync(HttpMethod.Delete, "v2/timelines/user/hello/members/notexist",
                expectedStatusCode: HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task MemberNotLogin()
        {
            using var client = CreateDefaultClient();
            await client.TestSendAsync(HttpMethod.Put, "v2/timelines/user/hello/members/admin", expectedStatusCode: HttpStatusCode.Unauthorized);
            await client.TestSendAsync(HttpMethod.Delete, "v2/timelines/user/hello/members/admin", expectedStatusCode: HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task MemberForbid()
        {
            await CreateUserAsync("user2", "user2pw");
            using var client = CreateClientWithToken(await CreateTokenWithCredentialAsync("user2", "user2pw"));
            await client.TestSendAsync(HttpMethod.Put, "v2/timelines/user/hello/members/admin", expectedStatusCode: HttpStatusCode.Forbidden);
            await client.TestSendAsync(HttpMethod.Delete, "v2/timelines/user/hello/members/admin", expectedStatusCode: HttpStatusCode.Forbidden);
        }
    }
}

