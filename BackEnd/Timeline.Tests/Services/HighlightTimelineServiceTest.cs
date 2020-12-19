using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;
using Timeline.Services;
using Timeline.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests.Services
{
    public class HighlightTimelineServiceTest : DatabaseBasedTest
    {
        private readonly TestClock _clock = new TestClock();
        private UserService _userService = default!;
        private TimelineService _timelineService = default!;

        private HighlightTimelineService _service = default!;

        public HighlightTimelineServiceTest(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {

        }

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

        [Fact]
        public async Task NewOne_Should_BeAtLast()
        {
            var userId = await _userService.GetUserIdByUsername("user");
            await _timelineService.CreateTimeline("t1", userId);
            await _service.AddHighlightTimeline("t1", userId);

            await _timelineService.CreateTimeline("t2", userId);
            await _service.AddHighlightTimeline("t2", userId);

            var ht = await _service.GetHighlightTimelines();

            ht.Should().HaveCount(2);
            ht[0].Name.Should().Be("t1");
            ht[1].Name.Should().Be("t2");
        }

        [Fact]
        public async Task Multiple_Should_Work()
        {
            var userId = await _userService.GetUserIdByUsername("user");

            // make timeline id not same as entity id.
            await _timelineService.CreateTimeline("t0", userId);

            await _timelineService.CreateTimeline("t1", userId);
            await _service.AddHighlightTimeline("t1", userId);

            await _timelineService.CreateTimeline("t2", userId);
            await _service.AddHighlightTimeline("t2", userId);

            await _timelineService.CreateTimeline("t3", userId);
            await _service.AddHighlightTimeline("t3", userId);

            await _service.MoveHighlightTimeline("t3", 2);
            (await _service.GetHighlightTimelines())[1].Name.Should().Be("t3");

            await _service.MoveHighlightTimeline("t1", 3);
            (await _service.GetHighlightTimelines())[2].Name.Should().Be("t1");

            await _service.RemoveHighlightTimeline("t2", userId);
            await _service.RemoveHighlightTimeline("t1", userId);
            await _service.RemoveHighlightTimeline("t3", userId);
            (await _service.GetHighlightTimelines()).Should().BeEmpty();
        }
    }
}
