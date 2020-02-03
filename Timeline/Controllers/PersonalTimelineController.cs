using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timeline.Filters;
using Timeline.Models.Http;
using Timeline.Models.Validation;
using Timeline.Services;

namespace Timeline.Controllers
{
    [ApiController]
    [CatchTimelineNotExistException]
    public class PersonalTimelineController : Controller
    {
        private readonly ILogger<PersonalTimelineController> _logger;

        private readonly IPersonalTimelineService _service;

        public PersonalTimelineController(ILogger<PersonalTimelineController> logger, IPersonalTimelineService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpGet("users/{username}/timeline")]
        public async Task<ActionResult<TimelineInfo>> TimelineGet([FromRoute][Username] string username)
        {
            return (await _service.GetTimeline(username)).FillLinksForPersonalTimeline(Url);
        }

        [HttpGet("users/{username}/timeline/posts")]
        public async Task<ActionResult<IList<TimelinePostInfo>>> PostListGet([FromRoute][Username] string username)
        {
            if (!this.IsAdministrator() && !await _service.HasReadPermission(username, this.GetOptionalUserId()))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            return await _service.GetPosts(username);
        }

        [HttpPost("users/{username}/timeline/posts")]
        [Authorize]
        public async Task<ActionResult<TimelinePostInfo>> PostPost([FromRoute][Username] string username, [FromBody] TimelinePostCreateRequest body)
        {
            var id = this.GetUserId();
            if (!this.IsAdministrator() && !await _service.IsMemberOf(username, id))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            var res = await _service.CreatePost(username, id, body.Content, body.Time);
            return res;
        }

        [HttpDelete("users/{username}/timeline/posts/{id}")]
        [Authorize]
        public async Task<ActionResult> PostDelete([FromRoute][Username] string username, [FromRoute] long id)
        {
            try
            {
                if (!this.IsAdministrator() && !await _service.HasPostModifyPermission(username, id, this.GetUserId()))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
                }
                await _service.DeletePost(username, id);
                return Ok(CommonDeleteResponse.Delete());
            }
            catch (TimelinePostNotExistException)
            {
                return Ok(CommonDeleteResponse.NotExist());
            }
        }

        [HttpPatch("users/{username}/timeline")]
        [Authorize]
        public async Task<ActionResult<TimelineInfo>> TimelinePatch([FromRoute][Username] string username, [FromBody] TimelinePatchRequest body)
        {
            if (!this.IsAdministrator() && !(await _service.HasManagePermission(username, this.GetUserId())))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }
            await _service.ChangeProperty(username, body);
            var timeline = (await _service.GetTimeline(username)).FillLinksForPersonalTimeline(Url);
            return Ok(timeline);
        }

        [HttpPut("users/{username}/timeline/members/{member}")]
        [Authorize]
        public async Task<ActionResult> TimelineMemberPut([FromRoute][Username] string username, [FromRoute][Username] string member)
        {
            if (!this.IsAdministrator() && !(await _service.HasManagePermission(username, this.GetUserId())))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            try
            {
                await _service.ChangeMember(username, new List<string> { member }, null);
                return Ok();
            }
            catch (UserNotExistException)
            {
                return BadRequest(ErrorResponse.TimelineCommon.MemberPut_NotExist());
            }
        }

        [HttpDelete("users/{username}/timeline/members/{member}")]
        [Authorize]
        public async Task<ActionResult> TimelineMemberDelete([FromRoute][Username] string username, [FromRoute][Username] string member)
        {
            if (!this.IsAdministrator() && !(await _service.HasManagePermission(username, this.GetUserId())))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            try
            {
                await _service.ChangeMember(username, null, new List<string> { member });
                return Ok(CommonDeleteResponse.Delete());
            }
            catch (UserNotExistException)
            {
                return Ok(CommonDeleteResponse.NotExist());
            }
        }
    }
}
