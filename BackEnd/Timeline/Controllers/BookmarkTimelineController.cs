using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Models.Http;
using Timeline.Models.Validation;
using Timeline.Services.Api;
using Timeline.Services.Mapper;
using Timeline.Services.Timeline;

namespace Timeline.Controllers
{
    /// <summary>
    /// Api related to timeline bookmarks.
    /// </summary>
    [ApiController]
    [ProducesErrorResponseType(typeof(CommonResponse))]
    public class BookmarkTimelineController : MyControllerBase
    {
        private readonly IBookmarkTimelineService _service;
        private readonly ITimelineService _timelineService;
        private readonly IGenericMapper _mapper;

        public BookmarkTimelineController(IBookmarkTimelineService service, ITimelineService timelineService, IGenericMapper mapper)
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
        /// Get bookmark list in order.
        /// </summary>
        /// <returns>Bookmarks.</returns>
        [HttpGet("bookmarks")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<List<HttpTimeline>>> List()
        {
            var ids = await _service.GetBookmarksAsync(GetUserId());
            var timelines = await _timelineService.GetTimelineList(ids);
            return await Map(timelines);
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
            var timelineId = await _timelineService.GetTimelineIdByNameAsync(timeline);
            var create = await _service.AddBookmarkAsync(GetUserId(), timelineId);
            return CommonPutResponse.Create(create);
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
            var timelineId = await _timelineService.GetTimelineIdByNameAsync(timeline);
            var delete = await _service.RemoveBookmarkAsync(GetUserId(), timelineId);
            return CommonDeleteResponse.Create(delete);
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
            var timelineId = await _timelineService.GetTimelineIdByNameAsync(request.Timeline);
            await _service.MoveBookmarkAsync(GetUserId(), timelineId, request.NewPosition!.Value);
            return OkWithCommonResponse();
        }
    }
}
