using System;
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

            a.Name.Should().Be(b.Name);
            a.UniqueId.Should().Be(b.UniqueId);
        }
    }
}

