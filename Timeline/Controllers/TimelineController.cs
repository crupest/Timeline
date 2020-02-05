using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Timeline.Filters;
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

        public TimelineController(ILogger<TimelineController> logger, IUserService userService, ITimelineService service)
        {
            _logger = logger;
            _userService = userService;
            _service = service;
        }

        [HttpGet("timelines")]
        public async Task<ActionResult<List<TimelineInfo>>> TimelineList([FromQuery][Username] string? relate, [FromQuery][RegularExpression("(own)|(join)")] string? relateType)
        {
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

            var result = await _service.GetTimelines(relationship);
            result.ForEach(t => t.FillLinks(Url));
            return Ok(result);
        }

        [HttpGet("timelines/{name}")]
        public async Task<ActionResult<TimelineInfo>> TimelineGet([FromRoute][TimelineName] string name)
        {
            var result = (await _service.GetTimeline(name)).FillLinks(Url);
            return Ok(result);
        }

        [HttpGet("timelines/{name}/posts")]
        public async Task<ActionResult<IList<TimelinePostInfo>>> PostListGet([FromRoute][TimelineName] string name)
        {
            if (!this.IsAdministrator() && !await _service.HasReadPermission(name, this.GetOptionalUserId()))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            return await _service.GetPosts(name);
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
        public async Task<ActionResult> PostDelete([FromRoute][TimelineName] string name, [FromRoute] long id)
        {
            try
            {
                if (!this.IsAdministrator() && !await _service.HasPostModifyPermission(name, id, this.GetUserId()))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
                }
                await _service.DeletePost(name, id);
                return Ok(CommonDeleteResponse.Delete());
            }
            catch (TimelinePostNotExistException)
            {
                return Ok(CommonDeleteResponse.NotExist());
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
    }
}
