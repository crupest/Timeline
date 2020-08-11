using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Models;
using Timeline.Services;
using Timeline.Tests.Helpers;
using Xunit;

namespace Timeline.Tests.Services
{
    public class TimelineServiceTest : IAsyncLifetime, IDisposable
    {
        private readonly TestDatabase _testDatabase = new TestDatabase();

        private DatabaseContext _databaseContext;

        private readonly PasswordService _passwordService = new PasswordService();

        private readonly ETagGenerator _eTagGenerator = new ETagGenerator();

        private readonly ImageValidator _imageValidator = new ImageValidator();

        private readonly TestClock _clock = new TestClock();

        private DataManager _dataManager;

        private UserService _userService;

        private TimelineService _timelineService;

        private UserDeleteService _userDeleteService;

        public TimelineServiceTest()
        {
        }

        public async Task InitializeAsync()
        {
            await _testDatabase.InitializeAsync();
            _databaseContext = _testDatabase.CreateContext();
            _dataManager = new DataManager(_databaseContext, _eTagGenerator);
            _userService = new UserService(NullLogger<UserService>.Instance, _databaseContext, _passwordService, _clock);
            _timelineService = new TimelineService(NullLogger<TimelineService>.Instance, _databaseContext, _dataManager, _userService, _imageValidator, _clock);
            _userDeleteService = new UserDeleteService(NullLogger<UserDeleteService>.Instance, _databaseContext, _timelineService);
        }

        public async Task DisposeAsync()
        {
            await _testDatabase.DisposeAsync();
            await _databaseContext.DisposeAsync();
        }

        public void Dispose()
        {
            _eTagGenerator.Dispose();
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

            void Check(Models.Timeline timeline)
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
        [InlineData("@user")]
        [InlineData("tl")]
        public async Task GetPosts_ModifiedSince(string timelineName)
        {
            _clock.ForwardCurrentTime();

            var userId = await _userService.GetUserIdByUsername("user");

            var _ = TimelineHelper.ExtractTimelineName(timelineName, out var isPersonal);
            if (!isPersonal)
                await _timelineService.CreateTimeline(timelineName, userId);

            var postContentList = new string[] { "a", "b", "c", "d" };

            DateTime testPoint = new DateTime();

            foreach (var (content, index) in postContentList.Select((v, i) => (v, i)))
            {
                var t = _clock.ForwardCurrentTime();
                if (index == 1)
                    testPoint = t;
                await _timelineService.CreateTextPost(timelineName, userId, content, null);
            }

            var posts = await _timelineService.GetPosts(timelineName, testPoint);
            posts.Should().HaveCount(3)
                .And.Subject.Select(p => (p.Content as TextTimelinePostContent).Text).Should().Equal(postContentList.Skip(1));
        }

        [Theory]
        [InlineData("@user")]
        [InlineData("tl")]
        public async Task GetPosts_IncludeDeleted(string timelineName)
        {
            var userId = await _userService.GetUserIdByUsername("user");

            var _ = TimelineHelper.ExtractTimelineName(timelineName, out var isPersonal);
            if (!isPersonal)
                await _timelineService.CreateTimeline(timelineName, userId);

            var postContentList = new string[] { "a", "b", "c", "d" };

            foreach (var content in postContentList)
            {
                await _timelineService.CreateTextPost(timelineName, userId, content, null);
            }

            var posts = await _timelineService.GetPosts(timelineName);
            posts.Should().HaveCount(4);
            posts.Select(p => p.Deleted).Should().Equal(Enumerable.Repeat(false, posts.Count));
            posts.Select(p => ((TextTimelinePostContent)p.Content).Text).Should().Equal(postContentList);

            foreach (var id in new long[] { posts[0].Id, posts[2].Id })
            {
                await _timelineService.DeletePost(timelineName, id);
            }

            posts = await _timelineService.GetPosts(timelineName);
            posts.Should().HaveCount(2);
            posts.Select(p => p.Deleted).Should().Equal(Enumerable.Repeat(false, posts.Count));
            posts.Select(p => ((TextTimelinePostContent)p.Content).Text).Should().Equal(new string[] { "b", "d" });

            posts = await _timelineService.GetPosts(timelineName, includeDeleted: true);
            posts.Should().HaveCount(4);
            posts.Select(p => p.Deleted).Should().Equal(new bool[] { true, false, true, false });
            posts.Where(p => !p.Deleted).Select(p => ((TextTimelinePostContent)p.Content).Text).Should().Equal(new string[] { "b", "d" });
        }

        [Theory]
        [InlineData("@admin")]
        [InlineData("tl")]
        public async Task GetPosts_ModifiedSince_UsernameChange(string timelineName)
        {
            var time1 = _clock.ForwardCurrentTime();

            var userId = await _userService.GetUserIdByUsername("user");

            var _ = TimelineHelper.ExtractTimelineName(timelineName, out var isPersonal);
            if (!isPersonal)
                await _timelineService.CreateTimeline(timelineName, userId);

            var postContentList = new string[] { "a", "b", "c", "d" };

            foreach (var (content, index) in postContentList.Select((v, i) => (v, i)))
            {
                await _timelineService.CreateTextPost(timelineName, userId, content, null);
            }

            var time2 = _clock.ForwardCurrentTime();

            {
                var posts = await _timelineService.GetPosts(timelineName, time2);
                posts.Should().HaveCount(0);
            }

            {
                await _userService.ModifyUser(userId, new User { Nickname = "haha" });
                var posts = await _timelineService.GetPosts(timelineName, time2);
                posts.Should().HaveCount(0);
            }

            {
                await _userService.ModifyUser(userId, new User { Username = "haha" });
                var posts = await _timelineService.GetPosts(timelineName, time2);
                posts.Should().HaveCount(4);
            }
        }

        [Theory]
        [InlineData("@admin")]
        [InlineData("tl")]
        public async Task GetPosts_ModifiedSince_UserDelete(string timelineName)
        {
            var time1 = _clock.ForwardCurrentTime();

            var userId = await _userService.GetUserIdByUsername("user");
            var adminId = await _userService.GetUserIdByUsername("admin");

            var _ = TimelineHelper.ExtractTimelineName(timelineName, out var isPersonal);
            if (!isPersonal)
                await _timelineService.CreateTimeline(timelineName, adminId);

            var postContentList = new string[] { "a", "b", "c", "d" };

            foreach (var (content, index) in postContentList.Select((v, i) => (v, i)))
            {
                await _timelineService.CreateTextPost(timelineName, userId, content, null);
            }

            var time2 = _clock.ForwardCurrentTime();

            {
                var posts = await _timelineService.GetPosts(timelineName, time2);
                posts.Should().HaveCount(0);
            }

            await _userDeleteService.DeleteUser("user");

            {
                var posts = await _timelineService.GetPosts(timelineName, time2);
                posts.Should().HaveCount(0);
            }

            {
                var posts = await _timelineService.GetPosts(timelineName, time2, true);
                posts.Should().HaveCount(4);
            }
        }
    }
}
