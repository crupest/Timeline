using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using TimelineApp.Filters;
using TimelineApp.Models;
using TimelineApp.Models.Http;
using TimelineApp.Models.Validation;
using TimelineApp.Services;

namespace TimelineApp.Controllers
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
        public async Task<ActionResult<TimelineInfo>> TimelineGet([FromRoute][TimelineName] string name)
        {
            var timeline = await _service.GetTimeline(name);
            var result = _mapper.Map<TimelineInfo>(timeline);
            return result;
        }

        [HttpGet("timelines/{name}/posts")]
        public async Task<ActionResult<List<TimelinePostInfo>>> PostListGet([FromRoute][TimelineName] string name)
        {
            if (!this.IsAdministrator() && !await _service.HasReadPermission(name, this.GetOptionalUserId()))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            var posts = await _service.GetPosts(name);
            var result = _mapper.Map<List<TimelinePostInfo>>(posts);

            return result;
        }

        [HttpPost("timelines/{name}/posts")]
        [Authorize]
        public async Task<ActionResult<TimelinePostInfo>> PostPost([FromRoute][TimelineName] string name, [FromBody] TimelinePostCreateRequest body)
        {
            var id = this.GetUserId();
            if (!this.IsAdministrator() && !await _service.IsMemberOf(name, id))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            var res = await _service.CreatePost(name, id, body.Content, body.Time);
            return res;
        }

        [HttpDelete("timelines/{name}/posts/{id}")]
        [Authorize]
        public async Task<ActionResult<CommonDeleteResponse>> PostDelete([FromRoute][TimelineName] string name, [FromRoute] long id)
        {
            try
            {
                if (!this.IsAdministrator() && !await _service.HasPostModifyPermission(name, id, this.GetUserId()))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
                }
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
        public async Task<ActionResult<TimelineInfo>> TimelinePatch([FromRoute][TimelineName] string name, [FromBody] TimelinePatchRequest body)
        {
            if (!this.IsAdministrator() && !(await _service.HasManagePermission(name, this.GetUserId())))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }
            await _service.ChangeProperty(name, body);
            var timeline = (await _service.GetTimeline(name)).FillLinks(Url);
            return Ok(timeline);
        }

        [HttpPut("timelines/{name}/members/{member}")]
        [Authorize]
        public async Task<ActionResult> TimelineMemberPut([FromRoute][TimelineName] string name, [FromRoute][Username] string member)
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
                return BadRequest(ErrorResponse.TimelineCommon.MemberPut_NotExist());
            }
        }

        [HttpDelete("timelines/{name}/members/{member}")]
        [Authorize]
        public async Task<ActionResult> TimelineMemberDelete([FromRoute][TimelineName] string name, [FromRoute][Username] string member)
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
                var timelineInfo = (await _service.CreateTimeline(body.Name, userId)).FillLinks(Url);
                return Ok(timelineInfo);
            }
            catch (ConflictException)
            {
                return BadRequest(ErrorResponse.TimelineCommon.NameConflict());
            }
        }

        [HttpDelete("timelines/{name}")]
        [Authorize]
        public async Task<ActionResult<TimelineInfo>> TimelineDelete([FromRoute][TimelineName] string name)
        {
            if (!this.IsAdministrator() && !(await _service.HasManagePermission(name, this.GetUserId())))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            try
            {
                await _service.DeleteTimeline(name);
                return Ok(CommonDeleteResponse.Delete());
            }
            catch (TimelineNotExistException)
            {
                return Ok(CommonDeleteResponse.NotExist());
            }
        }
    }
}
