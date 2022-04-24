using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Timeline.Helpers.Cache;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Models.Validation;
using Timeline.Services.Timeline;
using Timeline.Services.User;
using Timeline.SignalRHub;

namespace Timeline.Controllers.V2
{
    [ApiController]
    [Route("v2/timelines/{owner}/{timeline}/posts")]
    public class TimelinePostV2Controller : V2ControllerBase
    {
        private readonly ITimelineService _timelineService;
        private readonly ITimelinePostService _postService;

        private readonly MarkdownProcessor _markdownProcessor;

        private readonly IHubContext<TimelineHub> _timelineHubContext;

        public TimelinePostV2Controller(ITimelineService timelineService, ITimelinePostService timelinePostService, MarkdownProcessor markdownProcessor, IHubContext<TimelineHub> timelineHubContext)
        {
            _timelineService = timelineService;
            _postService = timelinePostService;
            _markdownProcessor = markdownProcessor;
            _timelineHubContext = timelineHubContext;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<Page<HttpTimelinePost>>> ListAsync([FromRoute][Username] string owner, [FromRoute][TimelineName] string timeline, [FromQuery] DateTime? modifiedSince,
                                                                          [FromQuery][PositiveInteger] int? page, [FromQuery][PositiveInteger] int? pageSize)
        {
            var timelineId = await _timelineService.GetTimelineIdAsync(owner, timeline);
            if (!UserHasPermission(UserPermission.AllTimelineManagement) && !await _timelineService.HasReadPermissionAsync(timelineId, GetOptionalAuthUserId()))
            {
                return Forbid();
            }
            var postPage = await _postService.GetPostsV2Async(timelineId, modifiedSince, page, pageSize);
            var items = await MapListAsync<HttpTimelinePost>(postPage.Items);
            return postPage.WithItems(items);
        }

        [HttpGet("{post}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status410Gone)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<HttpTimelinePost>> GetAsync([FromRoute][Username] string owner, [FromRoute][TimelineName] string timeline, [FromRoute(Name = "post")] long postId)
        {
            var timelineId = await _timelineService.GetTimelineIdAsync(owner, timeline);
            if (!UserHasPermission(UserPermission.AllTimelineManagement) && !await _timelineService.HasReadPermissionAsync(timelineId, GetOptionalAuthUserId()))
            {
                return Forbid();
            }
            var post = await _postService.GetPostV2Async(timelineId, postId);
            var result = await MapAsync<HttpTimelinePost>(post);
            return result;
        }

        [HttpGet("{post}/data")]
        [Produces(MimeTypes.ImagePng, MimeTypes.ImageJpeg, MimeTypes.ImageGif, MimeTypes.ImageWebp, MimeTypes.TextPlain, MimeTypes.TextMarkdown, MimeTypes.TextPlain, MimeTypes.ApplicationJson)]
        [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status410Gone)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ByteData>> DataIndexGetAsync([FromRoute][Username] string owner, [FromRoute][TimelineName] string timeline, [FromRoute] long post)
        {
            return await DataGetAsync(owner, timeline, post, 0);
        }

        [HttpGet("{post}/data/{data_index}")]
        [Produces(MimeTypes.ImagePng, MimeTypes.ImageJpeg, MimeTypes.ImageGif, MimeTypes.ImageWebp, MimeTypes.TextPlain, MimeTypes.TextMarkdown, MimeTypes.TextPlain, MimeTypes.ApplicationJson)]
        [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status410Gone)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> DataGetAsync([FromRoute][Username] string owner, [FromRoute][TimelineName] string timeline, [FromRoute] long post, [FromRoute(Name = "data_index")][Range(0, 100)] long dataIndex)
        {
            var timelineId = await _timelineService.GetTimelineIdAsync(owner, timeline);

            if (!UserHasPermission(UserPermission.AllTimelineManagement) && !await _timelineService.HasReadPermissionAsync(timelineId, GetOptionalAuthUserId()))
            {
                return Forbid();
            }

            return await DataCacheHelper.GenerateActionResult(this,
                () => _postService.GetPostDataDigestV2Async(timelineId, post, dataIndex),
                async () =>
                {
                    var data = await _postService.GetPostDataV2Async(timelineId, post, dataIndex);
                    if (data.ContentType == MimeTypes.TextMarkdown)
                    {
                        return new ByteData(_markdownProcessor.Process(data.Data, Url, owner, timeline, post), data.ContentType);
                    }
                    return data;
                }
            );
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<HttpTimelinePost>> PostAsync([FromRoute][Username] string owner, [FromRoute][TimelineName] string timeline, [FromBody] HttpTimelinePostCreateRequest body)
        {
            var timelineId = await _timelineService.GetTimelineIdAsync(owner, timeline);

            if (!UserHasPermission(UserPermission.AllTimelineManagement) && !await _timelineService.IsMemberOfAsync(timelineId, GetAuthUserId()))
            {
                return Forbid();
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
                    return UnprocessableEntity(new ErrorResponse(ErrorResponse.InvalidRequest, $"Data at index {i} is null."));

                try
                {
                    var d = Convert.FromBase64String(data.Data);
                    createRequest.DataList.Add(new TimelinePostCreateRequestData(data.ContentType, d));
                }
                catch (FormatException)
                {
                    return UnprocessableEntity(new ErrorResponse(ErrorResponse.InvalidRequest, $"Data at index {i} is not a valid base64 string."));
                }
            }

            try
            {
                var post = await _postService.CreatePostAsync(timelineId, GetAuthUserId(), createRequest);

                var group = TimelineHub.GenerateTimelinePostChangeListeningGroupName(owner, timeline);
                await _timelineHubContext.Clients.Group(group).SendAsync(nameof(ITimelineClient.OnTimelinePostChangedV2), timeline);

                var result = await MapAsync<HttpTimelinePost>(post);
                return CreatedAtAction("Get", new { owner = owner, timeline = timeline, post = post.LocalId }, result);
            }
            catch (TimelinePostCreateDataException e)
            {
                return UnprocessableEntity(new ErrorResponse(ErrorResponse.InvalidRequest, $"Data at index {e.Index} is invalid. {e.Message}"));
            }
        }

        [HttpPatch("{post}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<HttpTimelinePost>> Patch([FromRoute][Username] string owner, [FromRoute][TimelineName] string timeline, [FromRoute] long post, [FromBody] HttpTimelinePostPatchRequest body)
        {
            var timelineId = await _timelineService.GetTimelineIdAsync(owner, timeline);

            if (!UserHasPermission(UserPermission.AllTimelineManagement) && !await _postService.HasPostModifyPermissionAsync(timelineId, post, GetAuthUserId(), true))
            {
                return Forbid();
            }

            var entity = await _postService.PatchPostAsync(timelineId, post, new TimelinePostPatchRequest { Time = body.Time, Color = body.Color });
            var result = await MapAsync<HttpTimelinePost>(entity);

            return Ok(result);
        }

        [HttpDelete("{post}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> Delete([FromRoute][Username] string owner, [FromRoute][TimelineName] string timeline, [FromRoute] long post)
        {
            var timelineId = await _timelineService.GetTimelineIdAsync(owner, timeline);

            if (!UserHasPermission(UserPermission.AllTimelineManagement) && !await _postService.HasPostModifyPermissionAsync(timelineId, post, GetAuthUserId(), true))
            {
                return Forbid();
            }

            await _postService.DeletePostAsync(timelineId, post);

            return NoContent();
        }
    }
}

