﻿using FluentAssertions;
using System.Threading.Tasks;
using Timeline.Services.Api;
using Timeline.Services.Timeline;
using Timeline.Services.User;
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
            await TimelineService.CreateTimelineAsync(UserId, "hahaha");
            var t2 = await TimelineService.CreateTimelineAsync(UserId, "bababa");
            await TimelineService.ChangePropertyAsync(t2.Id, new TimelineChangePropertyParams { Title = "hahaha" });
            await TimelineService.CreateTimelineAsync(UserId, "bbbbbb");

            var searchResult = await _service.SearchTimelineAsync("hah");
            searchResult.Items.Should().HaveCount(2);
            searchResult.Items[0].Item.Name.Should().Be("hahaha");
            searchResult.Items[0].Score.Should().Be(2);
            searchResult.Items[1].Item.Name.Should().Be("bababa");
            searchResult.Items[1].Score.Should().Be(1);
        }

        [Fact]
        public async Task UserSearch_Should_Work()
        {
            await UserService.CreateUserAsync(new CreateUserParams("hahaha", "p"));
            var u2 = await UserService.CreateUserAsync(new CreateUserParams("bababa", "p"));
            await UserService.ModifyUserAsync(u2.Id, new ModifyUserParams { Nickname = "hahaha" });
            await UserService.CreateUserAsync(new CreateUserParams("bbbbbb", "p"));

            var searchResult = await _service.SearchUserAsync("hah");
            searchResult.Items.Should().HaveCount(2);
            searchResult.Items[0].Item.Username.Should().Be("hahaha");
            searchResult.Items[0].Score.Should().Be(2);
            searchResult.Items[1].Item.Username.Should().Be("bababa");
            searchResult.Items[1].Score.Should().Be(1);
        }
    }
}
