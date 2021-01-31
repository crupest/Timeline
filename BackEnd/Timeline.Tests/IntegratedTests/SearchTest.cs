using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timeline.Models.Http;
using Xunit;

namespace Timeline.Tests.IntegratedTests
{
    public class SearchTest : IntegratedTestBase
    {
        [Fact]
        public async Task TimelineSearch_Should_Work()
        {
            var client = await CreateClientAsUser();

            {
                await client.TestPostAsync("timelines", new HttpTimelineCreateRequest { Name = "hahaha" });
                await client.TestPostAsync("timelines", new HttpTimelineCreateRequest { Name = "bababa" });
                await client.TestPatchAsync("timelines/bababa", new HttpTimelinePatchRequest { Title = "hahaha" });
                await client.TestPostAsync("timelines", new HttpTimelineCreateRequest { Name = "gagaga" });
            }

            {
                var res = await client.TestGetAsync<List<HttpTimeline>>("search/timelines?q=hah");
                res.Should().HaveCount(2);
                res[0].Name.Should().Be("hahaha");
                res[1].Name.Should().Be("bababa");
            }

            {
                var res = await client.TestGetAsync<List<HttpTimeline>>("search/timelines?q=wuhu");
                res.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task UserSearch_Should_Work()
        {
            var client = await CreateClientAsAdministrator();

            {
                await client.TestPostAsync("users", new HttpCreateUserRequest { Username = "hahaha", Password = "p" });
                await client.TestPostAsync("users", new HttpCreateUserRequest { Username = "bababa", Password = "p" });
                await client.TestPatchAsync("users/bababa", new HttpUserPatchRequest { Nickname = "hahaha" });
                await client.TestPostAsync("users", new HttpCreateUserRequest { Username = "gagaga", Password = "p" });
            }

            {
                var res = await client.TestGetAsync<List<HttpUser>>("search/users?q=hah");
                res.Should().HaveCount(2);
                res[0].Username.Should().Be("hahaha");
                res[1].Username.Should().Be("bababa");
            }

            {
                var res = await client.TestGetAsync<List<HttpUser>>("search/users?q=wuhu");
                res.Should().BeEmpty();
            }
        }
    }
}
