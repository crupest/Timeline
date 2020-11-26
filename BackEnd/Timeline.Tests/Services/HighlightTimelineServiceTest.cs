using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;
using Timeline.Services;
using Timeline.Tests.Helpers;
using Xunit;

namespace Timeline.Tests.Services
{
    public class HighlightTimelineServiceTest : DatabaseBasedTest
    {
        private readonly TestClock _clock = new TestClock();
        private UserService _userService = default!;
        private TimelineService _timelineService = default!;

        private HighlightTimelineService _service = default!;

        protected override void OnDatabaseCreated()
        {
            _userService = new UserService(NullLogger<UserService>.Instance, Database, new PasswordService(), new UserPermissionService(Database), _clock);
            _timelineService = new TimelineService(Database, _userService, _clock);
            _service = new HighlightTimelineService(Database, _userService, _timelineService, _clock);
        }

        [Fact]
        public async Task Should_Work()
        {
            {
                var ht = await _service.GetHighlightTimelines();
                ht.Should().BeEmpty();
            }

            var userId = await _userService.GetUserIdByUsername("user");
            await _timelineService.CreateTimeline("tl", userId);
            await _service.AddHighlightTimeline("tl", userId);

            {
                var ht = await _service.GetHighlightTimelines();
                ht.Should().HaveCount(1).And.BeEquivalentTo(await _timelineService.GetTimeline("tl"));
            }
        }
    }
}
