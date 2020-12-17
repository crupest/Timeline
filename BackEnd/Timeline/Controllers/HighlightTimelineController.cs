using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timeline.Auth;
using Timeline.Models.Http;
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
        private readonly IMapper _mapper;

        public HighlightTimelineController(IHighlightTimelineService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all highlight timelines.
        /// </summary>
        /// <returns>Highlight timeline list.</returns>
        [HttpGet("highlights")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<List<HttpTimeline>>> List()
        {
            var t = await _service.GetHighlightTimelines();
            return _mapper.Map<List<HttpTimeline>>(t);
        }

        /// <summary>
        /// Add a timeline to highlight list.
        /// </summary>
        /// <param name="timeline">The timeline name.</param>
        [HttpPut("highlights/{timeline}")]
        [PermissionAuthorize(UserPermission.HighlightTimelineManagement)]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> Put([GeneralTimelineName] string timeline)
        {
            try
            {
                await _service.AddHighlightTimeline(timeline, this.GetUserId());
                return Ok();
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
        public async Task<ActionResult> Delete([GeneralTimelineName] string timeline)
        {
            try
            {
                await _service.RemoveHighlightTimeline(timeline, this.GetUserId());
                return Ok();
            }
            catch (TimelineNotExistException)
            {
                return BadRequest(ErrorResponse.TimelineController.NotExist());
            }
        }

        /// <summary>
        /// Move a highlight position.
        /// </summary>
        [HttpPost("highlightop/move")]
        [PermissionAuthorize(UserPermission.HighlightTimelineManagement)]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> Move([FromBody] HttpHighlightTimelineMoveRequest body)
        {
            try
            {
                await _service.MoveHighlightTimeline(body.Timeline, body.NewPosition!.Value);
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
