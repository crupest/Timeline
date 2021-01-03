using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;
using Timeline.Services;
using Timeline.Tests.Helpers;
using Xunit;

namespace Timeline.Tests.Services
{
    public class BookmarkTimelineServiceTest : DatabaseBasedTest
    {
        private BookmarkTimelineService _service = default!;
        private UserService _userService = default!;
        private TimelineService _timelineService = default!;

        protected override void OnDatabaseCreated()
        {
            var clock = new TestClock();
            _userService = new UserService(NullLogger<UserService>.Instance, Database, new PasswordService(), new UserPermissionService(Database), clock);
            _timelineService = new TimelineService(Database, _userService, clock);
            _service = new BookmarkTimelineService(Database, _userService, _timelineService);
        }

        [Fact]
        public async Task Should_Work()
        {
            var userId = await _userService.GetUserIdByUsername("user");

            {
                var b = await _service.GetBookmarks(userId);
                b.Should().BeEmpty();
            }

            await _timelineService.CreateTimeline("tl", userId);
            await _service.AddBookmark(userId, "tl");

            {
                var b = await _service.GetBookmarks(userId);
                b.Should().HaveCount(1).And.BeEquivalentTo(await _timelineService.GetTimeline("tl"));
            }
        }

        [Fact]
        public async Task NewOne_Should_BeAtLast()
        {
            var userId = await _userService.GetUserIdByUsername("user");
            await _timelineService.CreateTimeline("t1", userId);
            await _service.AddBookmark(userId, "t1");

            await _timelineService.CreateTimeline("t2", userId);
            await _service.AddBookmark(userId, "t2");

            var b = await _service.GetBookmarks(userId);

            b.Should().HaveCount(2);
            b[0].Name.Should().Be("t1");
            b[1].Name.Should().Be("t2");
        }

        [Fact]
        public async Task Multiple_Should_Work()
        {
            var userId = await _userService.GetUserIdByUsername("user");

            // make timeline id not same as entity id.
            await _timelineService.CreateTimeline("t0", userId);

            await _timelineService.CreateTimeline("t1", userId);
            await _service.AddBookmark(userId, "t1");

            await _timelineService.CreateTimeline("t2", userId);
            await _service.AddBookmark(userId, "t2");

            await _timelineService.CreateTimeline("t3", userId);
            await _service.AddBookmark(userId, "t3");

            await _service.MoveBookmark(userId, "t3", 2);
            (await _service.GetBookmarks(userId))[1].Name.Should().Be("t3");

            await _service.MoveBookmark(userId, "t1", 3);
            (await _service.GetBookmarks(userId))[2].Name.Should().Be("t1");

            await _service.RemoveBookmark(userId, "t2");
            await _service.RemoveBookmark(userId, "t1");
            await _service.RemoveBookmark(userId, "t3");
            (await _service.GetBookmarks(userId)).Should().BeEmpty();
        }

        [Fact]
        public async Task AddExist_Should_DoNothing()
        {
            var userId = await _userService.GetUserIdByUsername("user");

            await _timelineService.CreateTimeline("t", userId);

            await _service.AddBookmark(userId, "t");
            await _service.AddBookmark(userId, "t");

            (await _service.GetBookmarks(userId)).Should().HaveCount(1);
        }
    }
}
