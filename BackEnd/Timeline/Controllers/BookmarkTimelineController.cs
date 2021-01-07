﻿using Microsoft.AspNetCore.Authorization;
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

        public BookmarkTimelineController(IBookmarkTimelineService service, ITimelineService timelineService)
        {
            _service = service;
            _timelineService = timelineService;
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
            return Ok(timelines.MapToHttp(Url));
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
        public async Task<ActionResult> Put([GeneralTimelineName] string timeline)
        {
            try
            {
                await _service.AddBookmark(this.GetUserId(), timeline);
                return Ok();
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
        public async Task<ActionResult> Delete([GeneralTimelineName] string timeline)
        {
            try
            {
                await _service.RemoveBookmark(this.GetUserId(), timeline);
                return Ok();
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
                await _service.MoveBookmark(this.GetUserId(), request.Timeline, request.NewPosition!.Value);
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
