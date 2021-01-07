using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timeline.Models.Http;
using Timeline.Models.Mapper;
using Timeline.Models.Validation;
using Timeline.Services;
using Timeline.Services.Exceptions;

namespace Timeline.Controllers
{
    /// <summary>
    /// Api related to timeline bookmarks.
    /// </summary>
    [ApiController]
    [ProducesErrorResponseType(typeof(CommonResponse))]
    public class BookmarkTimelineController : Controller
    {
        private readonly IBookmarkTimelineService _service;
        private readonly ITimelineService _timelineService;
        private readonly TimelineMapper _timelineMapper;

        public BookmarkTimelineController(IBookmarkTimelineService service, ITimelineService timelineService, TimelineMapper timelineMapper)
        {
            _service = service;
            _timelineService = timelineService;
            _timelineMapper = timelineMapper;
        }

        /// <summary>
        /// Get bookmark list in order.
        /// </summary>
        /// <returns>Bookmarks.</returns>
        [HttpGet("bookmarks")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<List<HttpTimeline>>> List()
        {
            var ids = await _service.GetBookmarks(this.GetUserId());
            var timelines = await _timelineService.GetTimelineList(ids);
            return await _timelineMapper.MapToHttp(timelines, Url, this.GetOptionalUserId());
        }

        /// <summary>
        /// Add a bookmark.
        /// </summary>
        /// <param name="timeline">Timeline name.</param>
        [HttpPut("bookmarks/{timeline}")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<CommonPutResponse>> Put([GeneralTimelineName] string timeline)
        {
            try
            {
                var timelineId = await _timelineService.GetTimelineIdByName(timeline);
                var create = await _service.AddBookmark(this.GetUserId(), timelineId);
                return CommonPutResponse.Create(create);
            }
            catch (TimelineNotExistException)
            {
                return BadRequest(ErrorResponse.TimelineController.NotExist());
            }
        }

        /// <summary>
        /// Remove a bookmark.
        /// </summary>
        /// <param name="timeline">Timeline name.</param>
        [HttpDelete("bookmarks/{timeline}")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<CommonDeleteResponse>> Delete([GeneralTimelineName] string timeline)
        {
            try
            {
                var timelineId = await _timelineService.GetTimelineIdByName(timeline);
                var delete = await _service.RemoveBookmark(this.GetUserId(), timelineId);
                return CommonDeleteResponse.Create(delete);
            }
            catch (TimelineNotExistException)
            {
                return BadRequest(ErrorResponse.TimelineController.NotExist());
            }
        }

        /// <summary>
        /// Move a bookmark to new posisition.
        /// </summary>
        /// <param name="request">Request body.</param>
        [HttpPost("bookmarkop/move")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult> Move([FromBody] HttpBookmarkTimelineMoveRequest request)
        {
            try
            {
                var timelineId = await _timelineService.GetTimelineIdByName(request.Timeline);
                await _service.MoveBookmark(this.GetUserId(), timelineId, request.NewPosition!.Value);
                return Ok();
            }
            catch (TimelineNotExistException)
            {
                return BadRequest(ErrorResponse.TimelineController.NotExist());
            }
            catch (InvalidBookmarkException)
            {
                return BadRequest(new CommonResponse(ErrorCodes.BookmarkTimelineController.NonBookmark, "You can't move a non-bookmark timeline."));
            }
        }
    }
}
