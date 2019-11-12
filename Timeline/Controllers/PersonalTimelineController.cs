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

namespace Timeline
{
    public static partial class ErrorCodes
    {
        public static partial class Http
        {
            public static class Timeline // ccc = 004
            {
                public const int PostListGetForbid = 10040101;
                public const int PostOperationCreateForbid = 10040102;
                public const int PostOperationDeleteForbid = 10040103;
                public const int PostOperationDeleteNotExist = 10040201;
                public const int MemberAddNotExist = 10040301;
            }
        }
    }
}

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
                return StatusCode(StatusCodes.Status403Forbidden,
                    new CommonResponse(ErrorCodes.Http.Timeline.PostListGetForbid, MessagePostListGetForbid));
            }

            return await _service.GetPosts(username);
        }

        [HttpPost("user/{username}/timeline/postop/create")]
        [Authorize]
        [CatchTimelineNotExistException]
        public async Task<ActionResult<TimelinePostCreateResponse>> PostOperationCreate([FromRoute][Username] string username, [FromBody] TimelinePostCreateRequest body)
        {
            if (!IsAdmin() && !await _service.IsMemberOf(username, GetAuthUsername()!))
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new CommonResponse(ErrorCodes.Http.Timeline.PostOperationCreateForbid, MessagePostOperationCreateForbid));
            }

            var res = await _service.CreatePost(username, User.Identity.Name!, body.Content, body.Time);
            return res;
        }

        [HttpPost("user/{username}/timeline/postop/delete")]
        [Authorize]
        [CatchTimelineNotExistException]
        public async Task<ActionResult> PostOperationDelete([FromRoute][Username] string username, [FromBody] TimelinePostDeleteRequest body)
        {
            var postId = body.Id!.Value;
            if (!IsAdmin() && !await _service.HasPostModifyPermission(username, postId, GetAuthUsername()!))
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new CommonResponse(ErrorCodes.Http.Timeline.PostOperationDeleteForbid, MessagePostOperationCreateForbid));
            }
            try
            {
                await _service.DeletePost(username, postId);
            }
            catch (TimelinePostNotExistException)
            {
                return BadRequest(new CommonResponse(
                    ErrorCodes.Http.Timeline.PostOperationDeleteNotExist,
                    MessagePostOperationDeleteNotExist));
            }
            return Ok();
        }

        [HttpPost("user/{username}/timeline/op/property")]
        [Authorize]
        [SelfOrAdmin]
        [CatchTimelineNotExistException]
        public async Task<ActionResult> TimelineChangeProperty([FromRoute][Username] string username, [FromBody] TimelinePropertyChangeRequest body)
        {
            await _service.ChangeProperty(username, body);
            return Ok();
        }

        [HttpPost("user/{username}/timeline/op/member")]
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
                    return BadRequest(CommonResponse.InvalidModel(
                        string.Format(CultureInfo.CurrentCulture, MessageMemberUsernameBadFormat, e.Index, e.Operation)));
                }
                else if (e.InnerException is UserNotExistException)
                {
                    return BadRequest(new CommonResponse(ErrorCodes.Http.Timeline.MemberAddNotExist,
                        string.Format(CultureInfo.CurrentCulture, MessageMemberUserNotExist, e.Index, e.Operation)));
                }

                _logger.LogError(e, LogUnknownTimelineMemberOperationUserException);
                throw;
            }
        }
    }
}
