using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Services;

namespace Timeline.Tests.Services
{
    public class HighlightTimelineServiceTest : DatabaseBasedTest
    {
        private UserService _userService;
        private TimelineService _timelineService;

        private HighlightTimelineService _service;

        protected override void OnDatabaseCreated()
        {

        }

    }
}
