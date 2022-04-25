using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Timeline.Models.Http;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests.IntegratedTests2
{
    public class TimelineTest : IntegratedTestBase
    {
        public TimelineTest(ITestOutputHelper testOutput) : base(testOutput)
        {
        }

        [Fact]
        public async Task CreateAndGet()
        {
            using var client = CreateClientAsUser();
            var a = await client.TestJsonSendAsync<HttpTimeline>(HttpMethod.Post, "v2/timelines", new HttpTimelineCreateRequest
            {
                Name = "hello"
            }, expectedStatusCode: HttpStatusCode.Created);

            var b = await client.TestJsonSendAsync<HttpTimeline>(HttpMethod.Get, "v2/timelines/user/hello");

            a.NameV2.Should().Be(b.NameV2);
            a.UniqueId.Should().Be(b.UniqueId);
        }

        [Fact]
        public async Task CreateSameName()
        {
            using var client = CreateClientAsUser();
            await client.TestJsonSendAsync<HttpTimeline>(HttpMethod.Post, "v2/timelines", new HttpTimelineCreateRequest
            {
                Name = "hello"
            }, expectedStatusCode: HttpStatusCode.Created);

            await client.TestJsonSendAsync<HttpTimeline>(HttpMethod.Post, "v2/timelines", new HttpTimelineCreateRequest
            {
                Name = "hello"
            }, expectedStatusCode: HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task CreateInvalid()
        {
            using var client = CreateClientAsUser();
            await client.TestJsonSendAsync<HttpTimeline>(HttpMethod.Post, "v2/timelines", new HttpTimelineCreateRequest
            {
                Name = "!!!"
            }, expectedStatusCode: HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task DifferentUserCreateSameName()
        {
            using var userClient = CreateClientAsUser();
            await userClient.TestJsonSendAsync<HttpTimeline>(HttpMethod.Post, "v2/timelines", new HttpTimelineCreateRequest
            {
                Name = "hello"
            }, expectedStatusCode: HttpStatusCode.Created);

            using var adminClient = CreateClientAsAdmin();
            await adminClient.TestJsonSendAsync<HttpTimeline>(HttpMethod.Post, "v2/timelines", new HttpTimelineCreateRequest
            {
                Name = "hello"
            }, expectedStatusCode: HttpStatusCode.Created);
        }

        [Fact]
        public async Task CreateWithoutLogin()
        {
            using var client = CreateDefaultClient();
            await client.TestJsonSendAsync<HttpTimeline>(HttpMethod.Post, "v2/timelines", new HttpTimelineCreateRequest
            {
                Name = "hello"
            }, expectedStatusCode: HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetNotExist()
        {
            using var client = CreateDefaultClient();
            await client.TestJsonSendAsync<HttpTimeline>(HttpMethod.Get, "v2/timelines/notexist/notexist", expectedStatusCode: HttpStatusCode.NotFound);
            await client.TestJsonSendAsync<HttpTimeline>(HttpMethod.Get, "v2/timelines/user/notexist", expectedStatusCode: HttpStatusCode.NotFound);
        }
    }
}
