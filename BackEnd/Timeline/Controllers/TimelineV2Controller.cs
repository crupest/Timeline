using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Timeline.Entities;
using Timeline.Models.Http;
using Timeline.Services.Mapper;
using Timeline.Services.Timeline;

namespace Timeline.Controllers
{
    [ApiController]
    [Route("v2/timelines")]
    public class TimelineV2Controller : MyControllerBase
    {
        private ITimelineService _timelineService;
        private TimelineMapper _timelineMapper;

        public TimelineV2Controller(ITimelineService timelineService, TimelineMapper timelineMapper)
        {
            _timelineService = timelineService;
            _timelineMapper = timelineMapper;
        }

        [HttpGet("{owner}/{timeline}")]
        public async Task<ActionResult<HttpTimeline>> Get([FromRoute] string owner, [FromRoute] string timeline)
        {
            var timelineId = await _timelineService.GetTimelineIdAsync(owner, timeline);
            var t = await _timelineService.GetTimelineAsync(timelineId);
            return await _timelineMapper.MapAsync(t, Url, User);
        }
    }
}

