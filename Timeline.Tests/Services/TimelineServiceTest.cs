using Castle.Core.Logging;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Services;
using Timeline.Tests.Helpers;
using Xunit;

namespace Timeline.Tests.Services
{
    public class TimelineServiceTest : IAsyncLifetime
    {
        private TestDatabase _testDatabase = new TestDatabase();

        private DatabaseContext _databaseContext;

        private readonly PasswordService _passwordService = new PasswordService();

        private readonly ETagGenerator _eTagGenerator = new ETagGenerator();

        private readonly ImageValidator _imageValidator = new ImageValidator();

        private readonly TestClock _clock = new TestClock();

        private DataManager _dataManager;

        private UserService _userService;

        private TimelineService _timelineService;

        public TimelineServiceTest()
        {
        }

        public async Task InitializeAsync()
        {
            await _testDatabase.InitializeAsync();
            _databaseContext = _testDatabase.CreateContext();
            _dataManager = new DataManager(_databaseContext, _eTagGenerator);
            _userService = new UserService(NullLogger<UserService>.Instance, _databaseContext, _passwordService);
            _timelineService = new TimelineService(NullLogger<TimelineService>.Instance, _databaseContext, _dataManager, _userService, _imageValidator, _clock);
        }

        public async Task DisposeAsync()
        {
            await _testDatabase.DisposeAsync();
        }

        [Fact]
        public async Task PersonalTimeline_LastModified()
        {
            var mockTime = new DateTime(2000, 1, 1, 1, 1, 1);

            _clock.SetCurrentTime(mockTime);

            var timeline = await _timelineService.GetTimeline("@user");

            timeline.NameLastModified.Should().Be(mockTime);
            timeline.LastModified.Should().Be(mockTime);
        }

        [Fact]
        public async Task OrdinaryTimeline_LastModified()
        {
            var mockTime = new DateTime(2000, 1, 1, 1, 1, 1);

            _clock.SetCurrentTime(mockTime);

            {
                var timeline = await _timelineService.CreateTimeline("tl", await _userService.GetUserIdByUsername("user"));

                timeline.NameLastModified.Should().Be(mockTime);
                timeline.LastModified.Should().Be(mockTime);
            }

            {
                var timeline = await _timelineService.GetTimeline("tl");
                timeline.NameLastModified.Should().Be(mockTime);
                timeline.LastModified.Should().Be(mockTime);
            }
        }
    }
}
