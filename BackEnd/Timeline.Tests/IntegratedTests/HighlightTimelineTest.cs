using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timeline.Models.Http;
using Xunit;

namespace Timeline.Tests.IntegratedTests
{
    public class HighlightTimelineTest : IntegratedTestBase
    {
        [Fact]
        public async Task PermissionTest()
        {
            using var client = await CreateClientAsUser();

            await client.TestPutAssertForbiddenAsync("highlights/@user1");
            await client.TestDeleteAssertForbiddenAsync("highlights/@user1");
            await client.TestPostAssertForbiddenAsync("highlightop/move", new HttpHighlightTimelineMoveRequest { Timeline = "aaa", NewPosition = 1 });
        }

        [Fact]
        public async Task InvalidModel()
        {
            using var client = await CreateClientAsAdministrator();

            await client.TestPutAssertInvalidModelAsync("highlights/!!!");
            await client.TestDeleteAssertInvalidModelAsync("highlights/!!!");
            await client.TestPostAssertInvalidModelAsync("highlightop/move", new HttpHighlightTimelineMoveRequest { Timeline = null!, NewPosition = 1 });
            await client.TestPostAssertInvalidModelAsync("highlightop/move", new HttpHighlightTimelineMoveRequest { Timeline = "!!!", NewPosition = 1 });
            await client.TestPostAssertInvalidModelAsync("highlightop/move", new HttpHighlightTimelineMoveRequest { Timeline = "aaa", NewPosition = null });
        }

        [Fact]
        public async Task ShouldWork()
        {
            {
                using var client1 = await CreateClientAsUser();
                await client1.TestPostAsync("timelines", new HttpTimelineCreateRequest { Name = "t1" });
            }

            using var client = await CreateClientAsAdministrator();

            {
                var h = await client.TestGetAsync<List<HttpTimeline>>("highlights");
                h.Should().BeEmpty();
            }

            await client.TestPutAsync("highlights/@user1");

            {
                var h = await client.TestGetAsync<List<HttpTimeline>>("highlights");
                h.Should().HaveCount(1);
                h[0].Name.Should().Be("@user1");
            }

            await client.TestPutAsync("highlights/t1");

            {
                var h = await client.TestGetAsync<List<HttpTimeline>>("highlights");
                h.Should().HaveCount(2);
                h[0].Name.Should().Be("@user1");
                h[1].Name.Should().Be("t1");
            }

            await client.TestPostAsync("highlightop/move", new HttpHighlightTimelineMoveRequest { Timeline = "@user1", NewPosition = 2 });

            {
                var h = await client.TestGetAsync<List<HttpTimeline>>("highlights");
                h.Should().HaveCount(2);
                h[0].Name.Should().Be("t1");
                h[1].Name.Should().Be("@user1");
            }

            await client.TestDeleteAsync("highlights/@user1");

            {
                var h = await client.TestGetAsync<List<HttpTimeline>>("highlights");
                h.Should().HaveCount(1);
                h[0].Name.Should().Be("t1");
            }

            await client.TestDeleteAsync("highlights/t1");

            {
                var h = await client.TestGetAsync<List<HttpTimeline>>("highlights");
                h.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task TimelineGet_IsHighlighField_Should_Work()
        {
            using var client = await CreateClientAsAdministrator();
            await client.TestPostAsync("timelines", new HttpTimelineCreateRequest { Name = "t" });

            {
                var t = await client.TestGetAsync<HttpTimeline>("timelines/t");
                t.IsHighlight.Should().BeFalse();
            }

            await client.TestPutAsync("highlights/t");

            {
                var t = await client.TestGetAsync<HttpTimeline>("timelines/t");
                t.IsHighlight.Should().BeTrue();
            }

            {
                var client1 = await CreateDefaultClient();
                var t = await client1.TestGetAsync<HttpTimeline>("timelines/t");
                t.IsHighlight.Should().BeTrue();
            }

            await client.TestDeleteAsync("highlights/t");

            {
                var t = await client.TestGetAsync<HttpTimeline>("timelines/t");
                t.IsHighlight.Should().BeFalse();
            }

        }
    }
}
