using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timeline.Auth;
using Timeline.Entities;
using Timeline.Models.Http;
using Timeline.Models.Validation;
using Timeline.Services.Api;
using Timeline.Services.Mapper;
using Timeline.Services.Timeline;
using Timeline.Services.User;

namespace Timeline.Controllers
{
    /// <summary>
    /// Api related to highlight timeline.
    /// </summary>
    [ApiController]
    [ProducesErrorResponseType(typeof(CommonResponse))]
    public class HighlightTimelineController : Controller
    {
        private readonly IHighlightTimelineService _service;
        private readonly ITimelineService _timelineService;
        private readonly IGenericMapper _mapper;

        public HighlightTimelineController(IHighlightTimelineService service, ITimelineService timelineService, IGenericMapper mapper)
        {
            _service = service;
            _timelineService = timelineService;
            _mapper = mapper;
        }

        private Task<List<HttpTimeline>> Map(List<TimelineEntity> timelines)
        {
            return _mapper.MapListAsync<HttpTimeline>(timelines, Url, User);
        }

        /// <summary>
        /// Get all highlight timelines.
        /// </summary>
        /// <returns>Highlight timeline list.</returns>
        [HttpGet("highlights")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<List<HttpTimeline>>> List()
        {
            var ids = await _service.GetHighlightTimelinesAsync();
            var timelines = await _timelineService.GetTimelineList(ids);
            return await Map(timelines);
        }

        /// <summary>
        /// Add a timeline to highlight list.
        /// </summary>
        /// <param name="timeline">The timeline name.</param>
        [HttpPut("highlights/{timeline}")]
        [PermissionAuthorize(UserPermission.HighlightTimelineManagement)]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<CommonPutResponse>> Put([GeneralTimelineName] string timeline)
        {
            var timelineId = await _timelineService.GetTimelineIdByNameAsync(timeline);
            var create = await _service.AddHighlightTimelineAsync(timelineId, this.GetUserId());
            return CommonPutResponse.Create(create);
        }

        /// <summary>
        /// Remove a timeline from highlight list.
        /// </summary>
        /// <param name="timeline">Timeline name.</param>
        [HttpDelete("highlights/{timeline}")]
        [PermissionAuthorize(UserPermission.HighlightTimelineManagement)]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<CommonDeleteResponse>> Delete([GeneralTimelineName] string timeline)
        {
            var timelineId = await _timelineService.GetTimelineIdByNameAsync(timeline);
            var delete = await _service.RemoveHighlightTimelineAsync(timelineId, this.GetUserId());
            return CommonDeleteResponse.Create(delete);
        }

        /// <summary>
        /// Move a highlight to new position.
        /// </summary>
        [HttpPost("highlightop/move")]
        [PermissionAuthorize(UserPermission.HighlightTimelineManagement)]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult> Move([FromBody] HttpHighlightTimelineMoveRequest body)
        {
            try
            {
                var timelineId = await _timelineService.GetTimelineIdByNameAsync(body.Timeline);
                await _service.MoveHighlightTimelineAsync(timelineId, body.NewPosition!.Value);
                return Ok();
            }
            catch (InvalidHighlightTimelineException)
            {
                return BadRequest(new CommonResponse(ErrorCodes.HighlightTimelineController.NonHighlight, "Can't move a non-highlight timeline."));
            }
        }
    }
}
