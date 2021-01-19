using FluentAssertions;
using System.Threading.Tasks;
using Timeline.Services;
using Xunit;

namespace Timeline.Tests.Services
{
    public class SearchServiceTest : ServiceTestBase
    {
        private SearchService _service = default!;

        protected override void OnInitialize()
        {
            _service = new SearchService(Database);
        }

        [Fact]
        public async Task TimelineSearch_Should_Work()
        {
            await TimelineService.CreateTimeline("hahaha", UserId);
            var t2 = await TimelineService.CreateTimeline("bababa", UserId);
            await TimelineService.ChangeProperty(t2.Id, new TimelineChangePropertyParams { Title = "hahaha" });
            await TimelineService.CreateTimeline("bbbbbb", UserId);

            var searchResult = await _service.SearchTimeline("hah");
            searchResult.Items.Should().HaveCount(2);
            searchResult.Items[0].Item.Name.Should().Be("hahaha");
            searchResult.Items[0].Score.Should().Be(2);
            searchResult.Items[1].Item.Name.Should().Be("bababa");
            searchResult.Items[1].Score.Should().Be(1);
        }

        [Fact]
        public async Task UserSearch_Should_Work()
        {
            await UserService.CreateUser("hahaha", "p");
            var u2 = await UserService.CreateUser("bababa", "p");
            await UserService.ModifyUser(u2.Id, new ModifyUserParams { Nickname = "hahaha" });
            await UserService.CreateUser("bbbbbb", "p");

            var searchResult = await _service.SearchUser("hah");
            searchResult.Items.Should().HaveCount(2);
            searchResult.Items[0].Item.Username.Should().Be("hahaha");
            searchResult.Items[0].Score.Should().Be(2);
            searchResult.Items[1].Item.Username.Should().Be("bababa");
            searchResult.Items[1].Score.Should().Be(1);
        }
    }
}
