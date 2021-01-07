using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timeline.Auth;
using Timeline.Models.Http;
using Timeline.Models.Mapper;
using Timeline.Models.Validation;
using Timeline.Services;
using Timeline.Services.Exceptions;

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
        private readonly TimelineMapper _timelineMapper;

        public HighlightTimelineController(IHighlightTimelineService service, ITimelineService timelineService, TimelineMapper timelineMapper)
        {
            _service = service;
            _timelineService = timelineService;
            _timelineMapper = timelineMapper;
        }

        /// <summary>
        /// Get all highlight timelines.
        /// </summary>
        /// <returns>Highlight timeline list.</returns>
        [HttpGet("highlights")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<List<HttpTimeline>>> List()
        {
            var ids = await _service.GetHighlightTimelines();
            var timelines = await _timelineService.GetTimelineList(ids);
            return await _timelineMapper.MapToHttp(timelines, Url, this.GetOptionalUserId());
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
            try
            {
                var timelineId = await _timelineService.GetTimelineIdByName(timeline);
                var create = await _service.AddHighlightTimeline(timelineId, this.GetUserId());
                return CommonPutResponse.Create(create);
            }
            catch (TimelineNotExistException)
            {
                return BadRequest(ErrorResponse.TimelineController.NotExist());
            }
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
            try
            {
                var timelineId = await _timelineService.GetTimelineIdByName(timeline);
                var delete = await _service.RemoveHighlightTimeline(timelineId, this.GetUserId());
                return CommonDeleteResponse.Create(delete);
            }
            catch (TimelineNotExistException)
            {
                return BadRequest(ErrorResponse.TimelineController.NotExist());
            }
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
                var timelineId = await _timelineService.GetTimelineIdByName(body.Timeline);
                await _service.MoveHighlightTimeline(timelineId, body.NewPosition!.Value);
                return Ok();
            }
            catch (TimelineNotExistException)
            {
                return BadRequest(ErrorResponse.TimelineController.NotExist());
            }
            catch (InvalidHighlightTimelineException)
            {
                return BadRequest(new CommonResponse(ErrorCodes.HighlightTimelineController.NonHighlight, "Can't move a non-highlight timeline."));
            }
        }
    }
}
