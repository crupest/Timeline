using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Filters;
using Timeline.Helpers;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Models.Mapper;
using Timeline.Models.Validation;
using Timeline.Services;
using Timeline.Services.Exceptions;

namespace Timeline.Controllers
{
    /// <summary>
    /// Operations about timeline.
    /// </summary>
    [ApiController]
    [Route("timelines/{timeline}/posts")]
    [CatchTimelineNotExistException]
    [ProducesErrorResponseType(typeof(CommonResponse))]
    public class TimelinePostController : Controller
    {
        private readonly ITimelineService _timelineService;
        private readonly ITimelinePostService _postService;

        private readonly TimelineMapper _timelineMapper;

        /// <summary>
        /// 
        /// </summary>
        public TimelinePostController(ITimelineService timelineService, ITimelinePostService timelinePostService, TimelineMapper timelineMapper)
        {
            _timelineService = timelineService;
            _postService = timelinePostService;
            _timelineMapper = timelineMapper;
        }

        private bool UserHasAllTimelineManagementPermission => this.UserHasPermission(UserPermission.AllTimelineManagement);

        /// <summary>
        /// Get posts of a timeline.
        /// </summary>
        /// <param name="timeline">The name of the timeline.</param>
        /// <param name="modifiedSince">If set, only posts modified since the time will return.</param>
        /// <param name="includeDeleted">If set to true, deleted post will also return.</param>
        /// <returns>The post list.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<HttpTimelinePost>>> List([FromRoute][GeneralTimelineName] string timeline, [FromQuery] DateTime? modifiedSince, [FromQuery] bool? includeDeleted)
        {
            var timelineId = await _timelineService.GetTimelineIdByName(timeline);

            if (!UserHasAllTimelineManagementPermission && !await _timelineService.HasReadPermission(timelineId, this.GetOptionalUserId()))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            var posts = await _postService.GetPosts(timelineId, modifiedSince, includeDeleted ?? false);

            var result = await _timelineMapper.MapToHttp(posts, timeline, Url);
            return result;
        }

        /// <summary>
        /// Get a post of a timeline.
        /// </summary>
        /// <param name="timeline">The name of the timeline.</param>
        /// <param name="postId">The post id.</param>
        /// <returns>The post.</returns>
        [HttpGet("{post}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HttpTimelinePost>> Get([FromRoute][GeneralTimelineName] string timeline, [FromRoute(Name = "post")] long postId)
        {
            var timelineId = await _timelineService.GetTimelineIdByName(timeline);

            if (!UserHasAllTimelineManagementPermission && !await _timelineService.HasReadPermission(timelineId, this.GetOptionalUserId()))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            try
            {
                var post = await _postService.GetPost(timelineId, postId);
                var result = await _timelineMapper.MapToHttp(post, timeline, Url);
                return result;
            }
            catch (TimelinePostNotExistException)
            {
                return NotFound(ErrorResponse.TimelineController.PostNotExist());
            }
        }

        /// <summary>
        /// Get the data of a post. Usually a image post.
        /// </summary>
        /// <param name="timeline">Timeline name.</param>
        /// <param name="post">The id of the post.</param>
        /// <param name="ifNoneMatch">If-None-Match header.</param>
        /// <returns>The data.</returns>
        [HttpGet("{post}/data")]
        [Produces("image/png", "image/jpeg", "image/gif", "image/webp", "application/json", "text/json")]
        [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DataGet([FromRoute][GeneralTimelineName] string timeline, [FromRoute] long post, [FromHeader(Name = "If-None-Match")] string? ifNoneMatch)
        {
            _ = ifNoneMatch;

            var timelineId = await _timelineService.GetTimelineIdByName(timeline);

            if (!UserHasAllTimelineManagementPermission && !await _timelineService.HasReadPermission(timelineId, this.GetOptionalUserId()))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            try
            {
                return await DataCacheHelper.GenerateActionResult(this,
                    () => _postService.GetPostDataETag(timelineId, post),
                    async () => await _postService.GetPostData(timelineId, post));
            }
            catch (TimelinePostNotExistException)
            {
                return NotFound(ErrorResponse.TimelineController.PostNotExist());
            }
            catch (TimelinePostNoDataException)
            {
                return BadRequest(ErrorResponse.TimelineController.PostNoData());
            }
        }

        /// <summary>
        /// Create a new post.
        /// </summary>
        /// <param name="timeline">Timeline name.</param>
        /// <param name="body"></param>
        /// <returns>Info of new post.</returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<HttpTimelinePost>> Post([FromRoute][GeneralTimelineName] string timeline, [FromBody] HttpTimelinePostCreateRequest body)
        {
            var timelineId = await _timelineService.GetTimelineIdByName(timeline);
            var userId = this.GetUserId();

            if (!UserHasAllTimelineManagementPermission && !await _timelineService.IsMemberOf(timelineId, userId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            var content = body.Content;

            TimelinePostEntity post;

            TimelinePostCommonProperties properties = new TimelinePostCommonProperties { Color = body.Color, Time = body.Time };

            if (content.Type == TimelinePostContentTypes.Text)
            {
                var text = content.Text;
                if (text == null)
                {
                    return BadRequest(ErrorResponse.Common.CustomMessage_InvalidModel(Resources.Messages.TimelineController_TextContentTextRequired));
                }
                post = await _postService.CreateTextPost(timelineId, userId, text, properties);
            }
            else if (content.Type == TimelinePostContentTypes.Image)
            {
                var base64Data = content.Data;
                if (base64Data == null)
                {
                    return BadRequest(ErrorResponse.Common.CustomMessage_InvalidModel(Resources.Messages.TimelineController_ImageContentDataRequired));
                }
                byte[] data;
                try
                {
                    data = Convert.FromBase64String(base64Data);
                }
                catch (FormatException)
                {
                    return BadRequest(ErrorResponse.Common.CustomMessage_InvalidModel(Resources.Messages.TimelineController_ImageContentDataNotBase64));
                }

                try
                {
                    post = await _postService.CreateImagePost(timelineId, userId, data, properties);
                }
                catch (ImageException)
                {
                    return BadRequest(ErrorResponse.Common.CustomMessage_InvalidModel(Resources.Messages.TimelineController_ImageContentDataNotImage));
                }
            }
            else
            {
                return BadRequest(ErrorResponse.Common.CustomMessage_InvalidModel(Resources.Messages.TimelineController_ContentUnknownType));
            }

            var result = await _timelineMapper.MapToHttp(post, timeline, Url);
            return result;
        }

        /// <summary>
        /// Delete a post.
        /// </summary>
        /// <param name="timeline">Timeline name.</param>
        /// <param name="post">Post id.</param>
        /// <returns>Info of deletion.</returns>
        [HttpDelete("{post}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> Delete([FromRoute][GeneralTimelineName] string timeline, [FromRoute] long post)
        {
            var timelineId = await _timelineService.GetTimelineIdByName(timeline);

            try
            {
                if (!UserHasAllTimelineManagementPermission && !await _postService.HasPostModifyPermission(timelineId, post, this.GetUserId(), true))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
                }
                await _postService.DeletePost(timelineId, post);
                return Ok();
            }
            catch (TimelinePostNotExistException)
            {
                return BadRequest(ErrorResponse.TimelineController.PostNotExist());
            }
        }
    }
}
