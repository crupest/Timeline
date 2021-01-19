using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timeline.Models.Http;
using Xunit;

namespace Timeline.Tests.IntegratedTests
{
    public class BookmarkTimelineTest : IntegratedTestBase
    {
        [Fact]
        public async Task AuthTest()
        {
            using var client = await CreateDefaultClient();

            await client.TestPutAssertUnauthorizedAsync("bookmarks/@user1");
            await client.TestDeleteAssertUnauthorizedAsync("bookmarks/@user1");
            await client.TestPostAssertUnauthorizedAsync("bookmarkop/move", new HttpBookmarkTimelineMoveRequest { Timeline = "aaa", NewPosition = 1 });
        }

        [Fact]
        public async Task InvalidModel()
        {
            using var client = await CreateClientAsUser();

            await client.TestPutAssertInvalidModelAsync("bookmarks/!!!");
            await client.TestDeleteAssertInvalidModelAsync("bookmarks/!!!");
            await client.TestPostAssertInvalidModelAsync("bookmarkop/move", new HttpBookmarkTimelineMoveRequest { Timeline = null!, NewPosition = 1 });
            await client.TestPostAssertInvalidModelAsync("bookmarkop/move", new HttpBookmarkTimelineMoveRequest { Timeline = "!!!", NewPosition = 1 });
            await client.TestPostAssertInvalidModelAsync("bookmarkop/move", new HttpBookmarkTimelineMoveRequest { Timeline = "aaa", NewPosition = null });
        }

        [Fact]
        public async Task ShouldWork()
        {
            using var client = await CreateClientAsUser();
            await client.TestPostAsync("timelines", new HttpTimelineCreateRequest { Name = "t1" });


            {
                var h = await client.TestGetAsync<List<HttpTimeline>>("bookmarks");
                h.Should().BeEmpty();
            }

            await client.TestPutAsync("bookmarks/@user1");

            {
                var h = await client.TestGetAsync<List<HttpTimeline>>("bookmarks");
                h.Should().HaveCount(1);
                h[0].Name.Should().Be("@user1");
            }

            await client.TestPutAsync("bookmarks/t1");

            {
                var h = await client.TestGetAsync<List<HttpTimeline>>("bookmarks");
                h.Should().HaveCount(2);
                h[0].Name.Should().Be("@user1");
                h[1].Name.Should().Be("t1");
            }

            await client.TestPostAsync("bookmarkop/move", new HttpHighlightTimelineMoveRequest { Timeline = "@user1", NewPosition = 2 });

            {
                var h = await client.TestGetAsync<List<HttpTimeline>>("bookmarks");
                h.Should().HaveCount(2);
                h[0].Name.Should().Be("t1");
                h[1].Name.Should().Be("@user1");
            }

            await client.TestDeleteAsync("bookmarks/@user1");

            {
                var h = await client.TestGetAsync<List<HttpTimeline>>("bookmarks");
                h.Should().HaveCount(1);
                h[0].Name.Should().Be("t1");
            }

            await client.TestDeleteAsync("bookmarks/t1");

            {
                var h = await client.TestGetAsync<List<HttpTimeline>>("bookmarks");
                h.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task TimelineGet_IsBookmarkField_ShouldWork()
        {
            using var client = await CreateClientAsUser();
            await client.TestPostAsync("timelines", new HttpTimelineCreateRequest { Name = "t" });

            {
                var t = await client.TestGetAsync<HttpTimeline>("timelines/t");
                t.IsBookmark.Should().BeFalse();
            }

            await client.TestPutAsync("bookmarks/t");

            {
                var t = await client.TestGetAsync<HttpTimeline>("timelines/t");
                t.IsBookmark.Should().BeTrue();
            }

            {
                var client1 = await CreateDefaultClient();
                var t = await client1.TestGetAsync<HttpTimeline>("timelines/t");
                t.IsBookmark.Should().BeFalse();
            }

            {
                var client1 = await CreateClientAsAdministrator();
                var t = await client1.TestGetAsync<HttpTimeline>("timelines/t");
                t.IsBookmark.Should().BeFalse();
            }

            await client.TestDeleteAsync("bookmarks/t");

            {
                var t = await client.TestGetAsync<HttpTimeline>("timelines/t");
                t.IsBookmark.Should().BeFalse();
            }
        }
    }
}
