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

namespace Timeline.Controllers
{
    [ApiController]
    [CatchTimelineNotExistException]
    public class TimelineController : Controller
    {
        private readonly ILogger<TimelineController> _logger;

        private readonly IUserService _userService;
        private readonly ITimelineService _service;

        private readonly IMapper _mapper;

        public TimelineController(ILogger<TimelineController> logger, IUserService userService, ITimelineService service, IMapper mapper)
        {
            _logger = logger;
            _userService = userService;
            _service = service;
            _mapper = mapper;
        }

        [HttpGet("timelines")]
        public async Task<ActionResult<List<TimelineInfo>>> TimelineList([FromQuery][Username] string? relate, [FromQuery][RegularExpression("(own)|(join)")] string? relateType, [FromQuery] string? visibility)
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
            var result = _mapper.Map<List<TimelineInfo>>(timelines);
            return result;
        }

        [HttpGet("timelines/{name}")]
        public async Task<ActionResult<TimelineInfo>> TimelineGet([FromRoute][GeneralTimelineName] string name)
        {
            var timeline = await _service.GetTimeline(name);
            var result = _mapper.Map<TimelineInfo>(timeline);
            return result;
        }

        [HttpGet("timelines/{name}/posts")]
        public async Task<ActionResult<List<TimelinePostInfo>>> PostListGet([FromRoute][GeneralTimelineName] string name)
        {
            if (!this.IsAdministrator() && !await _service.HasReadPermission(name, this.GetOptionalUserId()))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            var posts = await _service.GetPosts(name);
            var result = _mapper.Map<List<TimelinePostInfo>>(posts);

            return result;
        }

        [HttpGet("timelines/{name}/posts/{id}/data")]
        public async Task<ActionResult<List<TimelinePostInfo>>> PostDataGet([FromRoute][GeneralTimelineName] string name, [FromRoute] long id)
        {
            if (!this.IsAdministrator() && !await _service.HasReadPermission(name, this.GetOptionalUserId()))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            try
            {
                return await DataCacheHelper.GenerateActionResult(this, () => _service.GetPostDataETag(name, id), async () =>
                {
                    var data = await _service.GetPostData(name, id);
                    return data;
                });
            }
            catch (TimelinePostNotExistException)
            {
                return NotFound(ErrorResponse.TimelineController.PostNotExist());
            }
            catch (BadPostTypeException)
            {
                return BadRequest(ErrorResponse.TimelineController.PostNoData());
            }
        }

        [HttpPost("timelines/{name}/posts")]
        [Authorize]
        public async Task<ActionResult<TimelinePostInfo>> PostPost([FromRoute][GeneralTimelineName] string name, [FromBody] TimelinePostCreateRequest body)
        {
            var id = this.GetUserId();
            if (!this.IsAdministrator() && !await _service.IsMemberOf(name, id))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            var content = body.Content;

            TimelinePost post;

            if (content.Type == TimelinePostContentTypes.Text)
            {
                var text = content.Text;
                if (text == null)
                {
                    return BadRequest(ErrorResponse.Common.CustomMessage_InvalidModel(Resources.Messages.TimelineController_TextContentTextRequired));
                }
                post = await _service.CreateTextPost(name, id, text, body.Time);
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
                    post = await _service.CreateImagePost(name, id, data, body.Time);
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

            var result = _mapper.Map<TimelinePostInfo>(post);
            return result;
        }

        [HttpDelete("timelines/{name}/posts/{id}")]
        [Authorize]
        public async Task<ActionResult<CommonDeleteResponse>> PostDelete([FromRoute][GeneralTimelineName] string name, [FromRoute] long id)
        {
            if (!this.IsAdministrator() && !await _service.HasPostModifyPermission(name, id, this.GetUserId()))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }
            try
            {
                await _service.DeletePost(name, id);
                return CommonDeleteResponse.Delete();
            }
            catch (TimelinePostNotExistException)
            {
                return CommonDeleteResponse.NotExist();
            }
        }

        [HttpPatch("timelines/{name}")]
        [Authorize]
        public async Task<ActionResult<TimelineInfo>> TimelinePatch([FromRoute][GeneralTimelineName] string name, [FromBody] TimelinePatchRequest body)
        {
            if (!this.IsAdministrator() && !(await _service.HasManagePermission(name, this.GetUserId())))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }
            await _service.ChangeProperty(name, _mapper.Map<TimelineChangePropertyRequest>(body));
            var timeline = await _service.GetTimeline(name);
            var result = _mapper.Map<TimelineInfo>(timeline);
            return result;
        }

        [HttpPut("timelines/{name}/members/{member}")]
        [Authorize]
        public async Task<ActionResult> TimelineMemberPut([FromRoute][GeneralTimelineName] string name, [FromRoute][Username] string member)
        {
            if (!this.IsAdministrator() && !(await _service.HasManagePermission(name, this.GetUserId())))
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

        [HttpDelete("timelines/{name}/members/{member}")]
        [Authorize]
        public async Task<ActionResult> TimelineMemberDelete([FromRoute][GeneralTimelineName] string name, [FromRoute][Username] string member)
        {
            if (!this.IsAdministrator() && !(await _service.HasManagePermission(name, this.GetUserId())))
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

        [HttpPost("timelines")]
        [Authorize]
        public async Task<ActionResult<TimelineInfo>> TimelineCreate([FromBody] TimelineCreateRequest body)
        {
            var userId = this.GetUserId();

            try
            {
                var timeline = await _service.CreateTimeline(body.Name, userId);
                var result = _mapper.Map<TimelineInfo>(timeline);
                return result;
            }
            catch (ConflictException)
            {
                return BadRequest(ErrorResponse.TimelineController.NameConflict());
            }
        }

        [HttpDelete("timelines/{name}")]
        [Authorize]
        public async Task<ActionResult<CommonDeleteResponse>> TimelineDelete([FromRoute][TimelineName] string name)
        {
            if (!this.IsAdministrator() && !(await _service.HasManagePermission(name, this.GetUserId())))
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
    }
}
