using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Timeline.Filters;
using Timeline.Helpers;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Models.Validation;
using Timeline.Services;
using Timeline.Services.Exceptions;

namespace Timeline.Controllers
{
    /// <summary>
    /// Operations about timeline.
    /// </summary>
    [ApiController]
    [CatchTimelineNotExistException]
    [ProducesErrorResponseType(typeof(CommonResponse))]
    public class TimelineController : Controller
    {
        private readonly ILogger<TimelineController> _logger;

        private readonly IUserService _userService;
        private readonly ITimelineService _service;
        private readonly ITimelinePostService _postService;

        private readonly IMapper _mapper;

        /// <summary>
        /// 
        /// </summary>
        public TimelineController(ILogger<TimelineController> logger, IUserService userService, ITimelineService service, ITimelinePostService timelinePostService, IMapper mapper)
        {
            _logger = logger;
            _userService = userService;
            _service = service;
            _postService = timelinePostService;
            _mapper = mapper;
        }

        private bool UserHasAllTimelineManagementPermission => this.UserHasPermission(UserPermission.AllTimelineManagement);

        /// <summary>
        /// List all timelines.
        /// </summary>
        /// <param name="relate">A username. If set, only timelines related to the user will return.</param>
        /// <param name="relateType">Specify the relation type, may be 'own' or 'join'. If not set, both type will return.</param>
        /// <param name="visibility">"Private" or "Register" or "Public". If set, only timelines whose visibility is specified one will return.</param>
        /// <returns>The timeline list.</returns>
        [HttpGet("timelines")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<HttpTimeline>>> TimelineList([FromQuery][Username] string? relate, [FromQuery][RegularExpression("(own)|(join)")] string? relateType, [FromQuery] string? visibility)
        {
            List<TimelineVisibility>? visibilityFilter = null;
            if (visibility != null)
            {
                visibilityFilter = new List<TimelineVisibility>();
                var items = visibility.Split('|');
                foreach (var item in items)
                {
                    if (item.Equals(nameof(TimelineVisibility.Private), StringComparison.OrdinalIgnoreCase))
                    {
                        if (!visibilityFilter.Contains(TimelineVisibility.Private))
                            visibilityFilter.Add(TimelineVisibility.Private);
                    }
                    else if (item.Equals(nameof(TimelineVisibility.Register), StringComparison.OrdinalIgnoreCase))
                    {
                        if (!visibilityFilter.Contains(TimelineVisibility.Register))
                            visibilityFilter.Add(TimelineVisibility.Register);
                    }
                    else if (item.Equals(nameof(TimelineVisibility.Public), StringComparison.OrdinalIgnoreCase))
                    {
                        if (!visibilityFilter.Contains(TimelineVisibility.Public))
                            visibilityFilter.Add(TimelineVisibility.Public);
                    }
                    else
                    {
                        return BadRequest(ErrorResponse.Common.CustomMessage_InvalidModel(Resources.Messages.TimelineController_QueryVisibilityUnknown, item));
                    }
                }
            }

            TimelineUserRelationship? relationship = null;
            if (relate != null)
            {
                try
                {
                    var relatedUserId = await _userService.GetUserIdByUsername(relate);

                    relationship = new TimelineUserRelationship(relateType switch
                    {
                        "own" => TimelineUserRelationshipType.Own,
                        "join" => TimelineUserRelationshipType.Join,
                        _ => TimelineUserRelationshipType.Default
                    }, relatedUserId);
                }
                catch (UserNotExistException)
                {
                    return BadRequest(ErrorResponse.TimelineController.QueryRelateNotExist());
                }
            }

            var timelines = await _service.GetTimelines(relationship, visibilityFilter);
            var result = _mapper.Map<List<HttpTimeline>>(timelines);
            return result;
        }

        /// <summary>
        /// Get info of a timeline.
        /// </summary>
        /// <param name="name">The timeline name.</param>
        /// <param name="checkUniqueId">A unique id. If specified and if-modified-since is also specified, the timeline info will return when unique id is not the specified one even if it is not modified.</param>
        /// <param name="queryIfModifiedSince">Same effect as If-Modified-Since header and take precedence than it.</param>
        /// <param name="headerIfModifiedSince">If specified, will return 304 if not modified.</param>
        /// <returns>The timeline info.</returns>
        [HttpGet("timelines/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HttpTimeline>> TimelineGet([FromRoute][GeneralTimelineName] string name, [FromQuery] string? checkUniqueId, [FromQuery(Name = "ifModifiedSince")] DateTime? queryIfModifiedSince, [FromHeader(Name = "If-Modified-Since")] DateTime? headerIfModifiedSince)
        {
            DateTime? ifModifiedSince = null;
            if (queryIfModifiedSince.HasValue)
            {
                ifModifiedSince = queryIfModifiedSince.Value;
            }
            else if (headerIfModifiedSince != null)
            {
                ifModifiedSince = headerIfModifiedSince.Value;
            }

            bool returnNotModified = false;

            if (ifModifiedSince.HasValue)
            {
                var lastModified = await _service.GetTimelineLastModifiedTime(name);
                if (lastModified < ifModifiedSince.Value)
                {
                    if (checkUniqueId != null)
                    {
                        var uniqueId = await _service.GetTimelineUniqueId(name);
                        if (uniqueId == checkUniqueId)
                        {
                            returnNotModified = true;
                        }
                    }
                    else
                    {
                        returnNotModified = true;
                    }
                }
            }

            if (returnNotModified)
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }
            else
            {
                var timeline = await _service.GetTimeline(name);
                var result = _mapper.Map<HttpTimeline>(timeline);
                return result;
            }
        }

        /// <summary>
        /// Get posts of a timeline.
        /// </summary>
        /// <param name="name">The name of the timeline.</param>
        /// <param name="modifiedSince">If set, only posts modified since the time will return.</param>
        /// <param name="includeDeleted">If set to true, deleted post will also return.</param>
        /// <returns>The post list.</returns>
        [HttpGet("timelines/{name}/posts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<HttpTimelinePost>>> PostListGet([FromRoute][GeneralTimelineName] string name, [FromQuery] DateTime? modifiedSince, [FromQuery] bool? includeDeleted)
        {
            if (!UserHasAllTimelineManagementPermission && !await _service.HasReadPermission(name, this.GetOptionalUserId()))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            List<TimelinePostInfo> posts = await _postService.GetPosts(name, modifiedSince, includeDeleted ?? false);

            var result = _mapper.Map<List<HttpTimelinePost>>(posts);
            return result;
        }

        /// <summary>
        /// Get the data of a post. Usually a image post.
        /// </summary>
        /// <param name="name">Timeline name.</param>
        /// <param name="id">The id of the post.</param>
        /// <param name="ifNoneMatch">If-None-Match header.</param>
        /// <returns>The data.</returns>
        [HttpGet("timelines/{name}/posts/{id}/data")]
        [Produces("image/png", "image/jpeg", "image/gif", "image/webp", "application/json", "text/json")]
        [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PostDataGet([FromRoute][GeneralTimelineName] string name, [FromRoute] long id, [FromHeader(Name = "If-None-Match")] string? ifNoneMatch)
        {
            _ = ifNoneMatch;
            if (!UserHasAllTimelineManagementPermission && !await _service.HasReadPermission(name, this.GetOptionalUserId()))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            try
            {
                return await DataCacheHelper.GenerateActionResult(this, () => _postService.GetPostDataETag(name, id), async () =>
                {
                    var data = await _postService.GetPostData(name, id);
                    return data;
                });
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
        /// <param name="name">Timeline name.</param>
        /// <param name="body"></param>
        /// <returns>Info of new post.</returns>
        [HttpPost("timelines/{name}/posts")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<HttpTimelinePost>> PostPost([FromRoute][GeneralTimelineName] string name, [FromBody] HttpTimelinePostCreateRequest body)
        {
            var id = this.GetUserId();
            if (!UserHasAllTimelineManagementPermission && !await _service.IsMemberOf(name, id))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            var content = body.Content;

            TimelinePostInfo post;

            if (content.Type == TimelinePostContentTypes.Text)
            {
                var text = content.Text;
                if (text == null)
                {
                    return BadRequest(ErrorResponse.Common.CustomMessage_InvalidModel(Resources.Messages.TimelineController_TextContentTextRequired));
                }
                post = await _postService.CreateTextPost(name, id, text, body.Time);
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
                    post = await _postService.CreateImagePost(name, id, data, body.Time);
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

            var result = _mapper.Map<HttpTimelinePost>(post);
            return result;
        }

        /// <summary>
        /// Delete a post.
        /// </summary>
        /// <param name="name">Timeline name.</param>
        /// <param name="id">Post id.</param>
        /// <returns>Info of deletion.</returns>
        [HttpDelete("timelines/{name}/posts/{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CommonDeleteResponse>> PostDelete([FromRoute][GeneralTimelineName] string name, [FromRoute] long id)
        {
            if (!UserHasAllTimelineManagementPermission && !await _postService.HasPostModifyPermission(name, id, this.GetUserId()))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }
            try
            {
                await _postService.DeletePost(name, id);
                return CommonDeleteResponse.Delete();
            }
            catch (TimelinePostNotExistException)
            {
                return CommonDeleteResponse.NotExist();
            }
        }

        /// <summary>
        /// Change properties of a timeline.
        /// </summary>
        /// <param name="name">Timeline name.</param>
        /// <param name="body"></param>
        /// <returns>The new info.</returns>
        [HttpPatch("timelines/{name}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<HttpTimeline>> TimelinePatch([FromRoute][GeneralTimelineName] string name, [FromBody] HttpTimelinePatchRequest body)
        {
            if (!UserHasAllTimelineManagementPermission && !(await _service.HasManagePermission(name, this.GetUserId())))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }
            await _service.ChangeProperty(name, _mapper.Map<TimelineChangePropertyRequest>(body));
            var timeline = await _service.GetTimeline(name);
            var result = _mapper.Map<HttpTimeline>(timeline);
            return result;
        }

        /// <summary>
        /// Add a member to timeline.
        /// </summary>
        /// <param name="name">Timeline name.</param>
        /// <param name="member">The new member's username.</param>
        [HttpPut("timelines/{name}/members/{member}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> TimelineMemberPut([FromRoute][GeneralTimelineName] string name, [FromRoute][Username] string member)
        {
            if (!UserHasAllTimelineManagementPermission && !(await _service.HasManagePermission(name, this.GetUserId())))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            try
            {
                await _service.ChangeMember(name, new List<string> { member }, null);
                return Ok();
            }
            catch (UserNotExistException)
            {
                return BadRequest(ErrorResponse.TimelineController.MemberPut_NotExist());
            }
        }

        /// <summary>
        /// Remove a member from timeline.
        /// </summary>
        /// <param name="name">Timeline name.</param>
        /// <param name="member">The member's username.</param>
        [HttpDelete("timelines/{name}/members/{member}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> TimelineMemberDelete([FromRoute][GeneralTimelineName] string name, [FromRoute][Username] string member)
        {
            if (!UserHasAllTimelineManagementPermission && !(await _service.HasManagePermission(name, this.GetUserId())))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            try
            {
                await _service.ChangeMember(name, null, new List<string> { member });
                return Ok(CommonDeleteResponse.Delete());
            }
            catch (UserNotExistException)
            {
                return Ok(CommonDeleteResponse.NotExist());
            }
        }

        /// <summary>
        /// Create a timeline.
        /// </summary>
        /// <param name="body"></param>
        /// <returns>Info of new timeline.</returns>
        [HttpPost("timelines")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<HttpTimeline>> TimelineCreate([FromBody] TimelineCreateRequest body)
        {
            var userId = this.GetUserId();

            try
            {
                var timeline = await _service.CreateTimeline(body.Name, userId);
                var result = _mapper.Map<HttpTimeline>(timeline);
                return result;
            }
            catch (EntityAlreadyExistException e) when (e.EntityName == EntityNames.Timeline)
            {
                return BadRequest(ErrorResponse.TimelineController.NameConflict());
            }
        }

        /// <summary>
        /// Delete a timeline.
        /// </summary>
        /// <param name="name">Timeline name.</param>
        /// <returns>Info of deletion.</returns>
        [HttpDelete("timelines/{name}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CommonDeleteResponse>> TimelineDelete([FromRoute][TimelineName] string name)
        {
            if (!UserHasAllTimelineManagementPermission && !(await _service.HasManagePermission(name, this.GetUserId())))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            try
            {
                await _service.DeleteTimeline(name);
                return CommonDeleteResponse.Delete();
            }
            catch (TimelineNotExistException)
            {
                return CommonDeleteResponse.NotExist();
            }
        }

        [HttpPost("timelineop/changename")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<HttpTimeline>> TimelineOpChangeName([FromBody] HttpTimelineChangeNameRequest body)
        {
            if (!UserHasAllTimelineManagementPermission && !(await _service.HasManagePermission(body.OldName, this.GetUserId())))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            try
            {
                var timeline = await _service.ChangeTimelineName(body.OldName, body.NewName);
                return Ok(_mapper.Map<HttpTimeline>(timeline));
            }
            catch (EntityAlreadyExistException)
            {
                return BadRequest(ErrorResponse.TimelineController.NameConflict());
            }
        }
    }
}
