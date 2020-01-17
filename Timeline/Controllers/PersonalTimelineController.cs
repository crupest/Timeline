using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Timeline.Auth;
using Timeline.Filters;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Models.Validation;
using Timeline.Services;
using static Timeline.Resources.Controllers.TimelineController;
using static Timeline.Resources.Messages;

namespace Timeline.Controllers
{
    [ApiController]
    public class PersonalTimelineController : Controller
    {
        private readonly ILogger<PersonalTimelineController> _logger;

        private readonly IPersonalTimelineService _service;

        private bool IsAdmin()
        {
            if (User != null)
            {
                return User.IsAdministrator();
            }
            return false;
        }

        private string? GetAuthUsername()
        {
            if (User == null)
            {
                return null;
            }
            else
            {
                return User.Identity.Name;
            }
        }

        public PersonalTimelineController(ILogger<PersonalTimelineController> logger, IPersonalTimelineService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpGet("users/{username}/timeline")]
        [CatchTimelineNotExistException]
        public async Task<ActionResult<BaseTimelineInfo>> TimelineGet([FromRoute][Username] string username)
        {
            return await _service.GetTimeline(username);
        }

        [HttpGet("users/{username}/timeline/posts")]
        [CatchTimelineNotExistException]
        public async Task<ActionResult<IList<TimelinePostInfo>>> PostListGet([FromRoute][Username] string username)
        {
            if (!IsAdmin() && !await _service.HasReadPermission(username, GetAuthUsername()))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            return await _service.GetPosts(username);
        }

        [HttpPost("users/{username}/timeline/postop/create")]
        [Authorize]
        [CatchTimelineNotExistException]
        public async Task<ActionResult<TimelinePostCreateResponse>> PostOperationCreate([FromRoute][Username] string username, [FromBody] TimelinePostCreateRequest body)
        {
            if (!IsAdmin() && !await _service.IsMemberOf(username, GetAuthUsername()!))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            var res = await _service.CreatePost(username, User.Identity.Name!, body.Content, body.Time);
            return res;
        }

        [HttpPost("users/{username}/timeline/postop/delete")]
        [Authorize]
        [CatchTimelineNotExistException]
        public async Task<ActionResult> PostOperationDelete([FromRoute][Username] string username, [FromBody] TimelinePostDeleteRequest body)
        {
            try
            {
                var postId = body.Id!.Value;
                if (!IsAdmin() && !await _service.HasPostModifyPermission(username, postId, GetAuthUsername()!))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
                }
                await _service.DeletePost(username, postId);
            }
            catch (TimelinePostNotExistException)
            {
                return BadRequest(ErrorResponse.TimelineController.PostOperationDelete_NotExist());
            }
            return Ok();
        }

        [HttpPost("users/{username}/timeline/op/property")]
        [Authorize]
        [SelfOrAdmin]
        [CatchTimelineNotExistException]
        public async Task<ActionResult> TimelineChangeProperty([FromRoute][Username] string username, [FromBody] TimelinePropertyChangeRequest body)
        {
            await _service.ChangeProperty(username, body);
            return Ok();
        }

        [HttpPost("users/{username}/timeline/op/member")]
        [Authorize]
        [SelfOrAdmin]
        [CatchTimelineNotExistException]
        public async Task<ActionResult> TimelineChangeMember([FromRoute][Username] string username, [FromBody] TimelineMemberChangeRequest body)
        {
            try
            {
                await _service.ChangeMember(username, body.Add, body.Remove);
                return Ok();
            }
            catch (TimelineMemberOperationUserException e)
            {
                if (e.InnerException is UsernameBadFormatException)
                {
                    return BadRequest(ErrorResponse.Common.CustomMessage_InvalidModel(
                        TimelineController_ChangeMember_UsernameBadFormat, e.Index, e.Operation));
                }
                else if (e.InnerException is UserNotExistException)
                {
                    return BadRequest(ErrorResponse.Common.CustomMessage_InvalidModel(
                        TimelineController_ChangeMember_UserNotExist, e.Index, e.Operation));
                }

                _logger.LogError(e, LogUnknownTimelineMemberOperationUserException);
                throw;
            }
        }
    }
}
