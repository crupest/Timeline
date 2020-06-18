using Castle.Core.Logging;
using FluentAssertions;
using FluentAssertions.Xml;
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
            await _databaseContext.DisposeAsync();
        }

        public void Dispose()
        {
            _eTagGenerator.Dispose();
        }

        [Theory]
        [InlineData("@user")]
        [InlineData("tl")]
        public async Task Timeline_LastModified(string timelineName)
        {
            _clock.ForwardCurrentTime();

            void Check(Models.Timeline timeline)
            {
                timeline.NameLastModified.Should().Be(_clock.GetCurrentTime());
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
    }
}
