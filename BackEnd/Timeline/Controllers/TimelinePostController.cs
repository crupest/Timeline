using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Filters;
using Timeline.Helpers.Cache;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Models.Validation;
using Timeline.Services.Mapper;
using Timeline.Services.Timeline;
using Timeline.Services.User;

namespace Timeline.Controllers
{
    /// <summary>
    /// Operations about timeline.
    /// </summary>
    [ApiController]
    [Route("timelines/{timeline}/posts")]
    [CatchTimelineNotExistException]
    [CatchTimelinePostNotExistException]
    [CatchTimelinePostDataNotExistException]
    [ProducesErrorResponseType(typeof(CommonResponse))]
    public class TimelinePostController : Controller
    {
        private readonly ITimelineService _timelineService;
        private readonly ITimelinePostService _postService;

        private readonly TimelineMapper _timelineMapper;

        private readonly MarkdownProcessor _markdownProcessor;

        /// <summary>
        /// 
        /// </summary>
        public TimelinePostController(ITimelineService timelineService, ITimelinePostService timelinePostService, TimelineMapper timelineMapper, MarkdownProcessor markdownProcessor)
        {
            _timelineService = timelineService;
            _postService = timelinePostService;
            _timelineMapper = timelineMapper;
            _markdownProcessor = markdownProcessor;
        }

        private bool UserHasAllTimelineManagementPermission => this.UserHasPermission(UserPermission.AllTimelineManagement);

        private Task<HttpTimelinePost> Map(TimelinePostEntity post, string timelineName)
        {
            return _timelineMapper.MapToHttp(post, timelineName, Url, this.GetOptionalUserId(), UserHasAllTimelineManagementPermission);
        }

        private Task<List<HttpTimelinePost>> Map(List<TimelinePostEntity> posts, string timelineName)
        {
            return _timelineMapper.MapToHttp(posts, timelineName, Url, this.GetOptionalUserId(), UserHasAllTimelineManagementPermission);
        }

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
            var timelineId = await _timelineService.GetTimelineIdByNameAsync(timeline);

            if (!UserHasAllTimelineManagementPermission && !await _timelineService.HasReadPermissionAsync(timelineId, this.GetOptionalUserId()))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            var posts = await _postService.GetPostsAsync(timelineId, modifiedSince, includeDeleted ?? false);

            var result = await Map(posts, timeline);
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
            var timelineId = await _timelineService.GetTimelineIdByNameAsync(timeline);

            if (!UserHasAllTimelineManagementPermission && !await _timelineService.HasReadPermissionAsync(timelineId, this.GetOptionalUserId()))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            var post = await _postService.GetPostAsync(timelineId, postId);
            var result = await Map(post, timeline);
            return result;
        }

        /// <summary>
        /// Get the first data of a post.
        /// </summary>
        /// <param name="timeline">Timeline name.</param>
        /// <param name="post">The id of the post.</param>
        /// <returns>The data.</returns>
        [HttpGet("{post}/data")]
        [Produces(MimeTypes.ImagePng, MimeTypes.ImageJpeg, MimeTypes.ImageGif, MimeTypes.ImageWebp, MimeTypes.TextPlain, MimeTypes.TextMarkdown, MimeTypes.TextPlain, MimeTypes.ApplicationJson)]
        [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ByteData>> DataIndexGet([FromRoute][GeneralTimelineName] string timeline, [FromRoute] long post)
        {
            return await DataGet(timeline, post, 0);
        }

        /// <summary>
        /// Get the data of a post. Usually a image post.
        /// </summary>
        /// <param name="timeline">Timeline name.</param>
        /// <param name="post">The id of the post.</param>
        /// <param name="dataIndex">Index of the data.</param>
        /// <returns>The data.</returns>
        [HttpGet("{post}/data/{data_index}")]
        [Produces(MimeTypes.ImagePng, MimeTypes.ImageJpeg, MimeTypes.ImageGif, MimeTypes.ImageWebp, MimeTypes.TextPlain, MimeTypes.TextMarkdown, MimeTypes.TextPlain, MimeTypes.ApplicationJson)]
        [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DataGet([FromRoute][GeneralTimelineName] string timeline, [FromRoute] long post, [FromRoute(Name = "data_index")][Range(0, 100)] long dataIndex)
        {
            var timelineId = await _timelineService.GetTimelineIdByNameAsync(timeline);

            if (!UserHasAllTimelineManagementPermission && !await _timelineService.HasReadPermissionAsync(timelineId, this.GetOptionalUserId()))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            return await DataCacheHelper.GenerateActionResult(this,
                () => _postService.GetPostDataDigestAsync(timelineId, post, dataIndex),
                async () =>
                {
                    var data = await _postService.GetPostDataAsync(timelineId, post, dataIndex);
                    if (data.ContentType == MimeTypes.TextMarkdown)
                    {
                        return new ByteData(_markdownProcessor.Process(data.Data, Url, timeline, post), data.ContentType);
                    }
                    return data;
                }
            );
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
            var timelineId = await _timelineService.GetTimelineIdByNameAsync(timeline);
            var userId = this.GetUserId();

            if (!UserHasAllTimelineManagementPermission && !await _timelineService.IsMemberOfAsync(timelineId, userId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            var createRequest = new TimelinePostCreateRequest()
            {
                Time = body.Time,
                Color = body.Color
            };

            for (int i = 0; i < body.DataList.Count; i++)
            {
                var data = body.DataList[i];

                if (data is null)
                    return BadRequest(new CommonResponse(ErrorCodes.Common.InvalidModel, $"Data at index {i} is null."));

                try
                {
                    var d = Convert.FromBase64String(data.Data);
                    createRequest.DataList.Add(new TimelinePostCreateRequestData(data.ContentType, d));
                }
                catch (FormatException)
                {
                    return BadRequest(new CommonResponse(ErrorCodes.Common.InvalidModel, $"Data at index {i} is not a valid base64 string."));
                }
            }


            try
            {
                var post = await _postService.CreatePostAsync(timelineId, userId, createRequest);
                var result = await Map(post, timeline);
                return result;
            }
            catch (TimelinePostCreateDataException e)
            {
                return BadRequest(new CommonResponse(ErrorCodes.Common.InvalidModel, $"Data at index {e.Index} is invalid. {e.Message}"));
            }
        }

        /// <summary>
        /// Update a post except content.
        /// </summary>
        /// <param name="timeline">Timeline name.</param>
        /// <param name="post">Post id.</param>
        /// <param name="body">Request body.</param>
        /// <returns>New info of post.</returns>
        [HttpPatch("{post}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<HttpTimelinePost>> Patch([FromRoute][GeneralTimelineName] string timeline, [FromRoute] long post, [FromBody] HttpTimelinePostPatchRequest body)
        {
            var timelineId = await _timelineService.GetTimelineIdByNameAsync(timeline);

            if (!UserHasAllTimelineManagementPermission && !await _postService.HasPostModifyPermissionAsync(timelineId, post, this.GetUserId(), true))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            var entity = await _postService.PatchPostAsync(timelineId, post, new TimelinePostPatchRequest { Time = body.Time, Color = body.Color });
            var result = await Map(entity, timeline);

            return Ok(result);
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
            var timelineId = await _timelineService.GetTimelineIdByNameAsync(timeline);

            if (!UserHasAllTimelineManagementPermission && !await _postService.HasPostModifyPermissionAsync(timelineId, post, this.GetUserId(), true))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            await _postService.DeletePostAsync(timelineId, post);

            return Ok();
        }
    }
}
