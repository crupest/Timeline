using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timeline.Models;
using Timeline.Services;
using Timeline.Services.Exceptions;
using Timeline.Tests.Helpers;
using Xunit;

namespace Timeline.Tests.Services
{
    public class TimelineServiceTest : DatabaseBasedTest
    {
        private readonly PasswordService _passwordService = new PasswordService();

        private readonly TestClock _clock = new TestClock();

        private UserPermissionService _userPermissionService = default!;

        private UserService _userService = default!;

        private TimelineService _timelineService = default!;

        protected override void OnDatabaseCreated()
        {
            _userPermissionService = new UserPermissionService(Database);
            _userService = new UserService(NullLogger<UserService>.Instance, Database, _passwordService, _userPermissionService, _clock);
            _timelineService = new TimelineService(Database, _userService, _clock);
        }

        [Theory]
        [InlineData("@user")]
        [InlineData("tl")]
        public async Task Timeline_GetLastModified(string timelineName)
        {
            var time = _clock.ForwardCurrentTime();

            var _ = TimelineHelper.ExtractTimelineName(timelineName, out var isPersonal);
            if (!isPersonal)
                await _timelineService.CreateTimeline(timelineName, await _userService.GetUserIdByUsername("user"));

            var t = await _timelineService.GetTimelineLastModifiedTime(timelineName);

            t.Should().Be(time);
        }

        [Theory]
        [InlineData("@user")]
        [InlineData("tl")]
        public async Task Timeline_GetUnqiueId(string timelineName)
        {
            var _ = TimelineHelper.ExtractTimelineName(timelineName, out var isPersonal);
            if (!isPersonal)
                await _timelineService.CreateTimeline(timelineName, await _userService.GetUserIdByUsername("user"));

            var uniqueId = await _timelineService.GetTimelineUniqueId(timelineName);

            uniqueId.Should().NotBeNullOrEmpty();
        }

        [Theory]
        [InlineData("@user")]
        [InlineData("tl")]
        public async Task Timeline_LastModified(string timelineName)
        {
            var initTime = _clock.ForwardCurrentTime();

            void Check(TimelineInfo timeline)
            {
                timeline.NameLastModified.Should().Be(initTime);
                timeline.LastModified.Should().Be(_clock.GetCurrentTime());
            }

            async Task GetAndCheck()
            {
                Check(await _timelineService.GetTimeline(timelineName));
            }

            var _ = TimelineHelper.ExtractTimelineName(timelineName, out var isPersonal);
            if (!isPersonal)
                Check(await _timelineService.CreateTimeline(timelineName, await _userService.GetUserIdByUsername("user")));

            await GetAndCheck();

            _clock.ForwardCurrentTime();
            await _timelineService.ChangeProperty(timelineName, new TimelineChangePropertyRequest { Visibility = TimelineVisibility.Public });
            await GetAndCheck();

            _clock.ForwardCurrentTime();
            await _timelineService.ChangeMember(timelineName, new List<string> { "admin" }, null);
            await GetAndCheck();
        }

        [Theory]
        [InlineData("@admin")]
        [InlineData("tl")]
        public async Task Title(string timelineName)
        {
            var _ = TimelineHelper.ExtractTimelineName(timelineName, out var isPersonal);
            if (!isPersonal)
                await _timelineService.CreateTimeline(timelineName, await _userService.GetUserIdByUsername("user"));

            {
                var timeline = await _timelineService.GetTimeline(timelineName);
                timeline.Title.Should().Be(timelineName);
            }

            {
                await _timelineService.ChangeProperty(timelineName, new TimelineChangePropertyRequest { Title = null });
                var timeline = await _timelineService.GetTimeline(timelineName);
                timeline.Title.Should().Be(timelineName);
            }

            {
                await _timelineService.ChangeProperty(timelineName, new TimelineChangePropertyRequest { Title = "atitle" });
                var timeline = await _timelineService.GetTimeline(timelineName);
                timeline.Title.Should().Be("atitle");
            }
        }

        [Fact]
        public async Task ChangeName()
        {
            _clock.ForwardCurrentTime();

            await _timelineService.Awaiting(s => s.ChangeTimelineName("!!!", "newtl")).Should().ThrowAsync<ArgumentException>();
            await _timelineService.Awaiting(s => s.ChangeTimelineName("tl", "!!!")).Should().ThrowAsync<ArgumentException>();
            await _timelineService.Awaiting(s => s.ChangeTimelineName("tl", "newtl")).Should().ThrowAsync<TimelineNotExistException>();

            await _timelineService.CreateTimeline("tl", await _userService.GetUserIdByUsername("user"));
            await _timelineService.CreateTimeline("tl2", await _userService.GetUserIdByUsername("user"));

            await _timelineService.Awaiting(s => s.ChangeTimelineName("tl", "tl2")).Should().ThrowAsync<EntityAlreadyExistException>();

            var time = _clock.ForwardCurrentTime();

            await _timelineService.ChangeTimelineName("tl", "newtl");

            {
                var timeline = await _timelineService.GetTimeline("newtl");
                timeline.Name.Should().Be("newtl");
                timeline.LastModified.Should().Be(time);
                timeline.NameLastModified.Should().Be(time);
            }
        }
    }
}
