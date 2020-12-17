using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Models;
using Timeline.Services;
using Timeline.Tests.Helpers;
using Xunit;

namespace Timeline.Tests.Services
{
    public class TimelinePostServiceTest : DatabaseBasedTest
    {
        private readonly PasswordService _passwordService = new PasswordService();

        private readonly ETagGenerator _eTagGenerator = new ETagGenerator();

        private readonly ImageValidator _imageValidator = new ImageValidator();

        private readonly TestClock _clock = new TestClock();

        private DataManager _dataManager = default!;

        private UserPermissionService _userPermissionService = default!;

        private UserService _userService = default!;

        private TimelineService _timelineService = default!;

        private TimelinePostService _timelinePostService = default!;

        private UserDeleteService _userDeleteService = default!;

        protected override void OnDatabaseCreated()
        {
            _dataManager = new DataManager(Database, _eTagGenerator);
            _userPermissionService = new UserPermissionService(Database);
            _userService = new UserService(NullLogger<UserService>.Instance, Database, _passwordService, _userPermissionService, _clock);
            _timelineService = new TimelineService(Database, _userService, _clock);
            _timelinePostService = new TimelinePostService(NullLogger<TimelinePostService>.Instance, Database, _timelineService, _userService, _dataManager, _imageValidator, _clock);
            _userDeleteService = new UserDeleteService(NullLogger<UserDeleteService>.Instance, Database, _timelinePostService);
        }

        protected override void BeforeDatabaseDestroy()
        {
            _eTagGenerator.Dispose();
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
                await _timelinePostService.CreateTextPost(timelineName, userId, content, null);
            }

            var posts = await _timelinePostService.GetPosts(timelineName, testPoint);
            posts.Should().HaveCount(3)
                .And.Subject.Select(p => ((TextTimelinePostContent)p.Content!).Text).Should().Equal(postContentList.Skip(1));
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
                await _timelinePostService.CreateTextPost(timelineName, userId, content, null);
            }

            var posts = await _timelinePostService.GetPosts(timelineName);
            posts.Should().HaveCount(4);
            posts.Select(p => p.Deleted).Should().Equal(Enumerable.Repeat(false, posts.Count));
            posts.Select(p => ((TextTimelinePostContent)p.Content!).Text).Should().Equal(postContentList);

            foreach (var id in new long[] { posts[0].Id, posts[2].Id })
            {
                await _timelinePostService.DeletePost(timelineName, id);
            }

            posts = await _timelinePostService.GetPosts(timelineName);
            posts.Should().HaveCount(2);
            posts.Select(p => p.Deleted).Should().Equal(Enumerable.Repeat(false, posts.Count));
            posts.Select(p => ((TextTimelinePostContent)p.Content!).Text).Should().Equal(new string[] { "b", "d" });

            posts = await _timelinePostService.GetPosts(timelineName, includeDeleted: true);
            posts.Should().HaveCount(4);
            posts.Select(p => p.Deleted).Should().Equal(new bool[] { true, false, true, false });
            posts.Where(p => !p.Deleted).Select(p => ((TextTimelinePostContent)p.Content!).Text).Should().Equal(new string[] { "b", "d" });
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
                await _timelinePostService.CreateTextPost(timelineName, userId, content, null);
            }

            var time2 = _clock.ForwardCurrentTime();

            {
                var posts = await _timelinePostService.GetPosts(timelineName, time2);
                posts.Should().HaveCount(0);
            }

            {
                await _userService.ModifyUser(userId, new ModifyUserParams { Nickname = "haha" });
                var posts = await _timelinePostService.GetPosts(timelineName, time2);
                posts.Should().HaveCount(0);
            }

            {
                await _userService.ModifyUser(userId, new ModifyUserParams { Username = "haha" });
                var posts = await _timelinePostService.GetPosts(timelineName, time2);
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
                await _timelinePostService.CreateTextPost(timelineName, userId, content, null);
            }

            var time2 = _clock.ForwardCurrentTime();

            {
                var posts = await _timelinePostService.GetPosts(timelineName, time2);
                posts.Should().HaveCount(0);
            }

            await _userDeleteService.DeleteUser("user");

            {
                var posts = await _timelinePostService.GetPosts(timelineName, time2);
                posts.Should().HaveCount(0);
            }

            {
                var posts = await _timelinePostService.GetPosts(timelineName, time2, true);
                posts.Should().HaveCount(4);
            }
        }
    }
}
