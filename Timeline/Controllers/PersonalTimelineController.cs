using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Auth;
using Timeline.Entities;
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
                public const int PostsGetForbid = 10040101;
                public const int PostsCreateForbid = 10040102;
            }
        }
    }
}

namespace Timeline.Controllers
{
    [ApiController]
    public class PersonalTimelineController : Controller
    {
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

        public PersonalTimelineController(IPersonalTimelineService service)
        {
            _service = service;
        }

        [HttpGet("users/{username}/timeline")]
        public async Task<ActionResult<BaseTimelineInfo>> TimelineGet([FromRoute][Username] string username)
        {
            return await _service.GetTimeline(username);
        }

        [HttpGet("users/{username}/timeline/posts")]
        public async Task<ActionResult<IList<TimelinePostInfo>>> PostsGet([FromRoute][Username] string username)
        {
            if (!IsAdmin() && !await _service.HasReadPermission(username, GetAuthUsername()))
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new CommonResponse(ErrorCodes.Http.Timeline.PostsGetForbid, MessagePostsGetForbid));
            }

            return await _service.GetPosts(username);
        }

        [HttpPost("user/{username}/timeline/posts/create")]
        [Authorize]
        public async Task<ActionResult> PostsCreate([FromRoute][Username] string username, [FromBody] TimelinePostCreateRequest body)
        {
            if (!IsAdmin() && !await _service.IsMemberOf(username, GetAuthUsername()!))
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new CommonResponse(ErrorCodes.Http.Timeline.PostsCreateForbid, MessagePostsCreateForbid));
            }

            await _service.CreatePost(username, User.Identity.Name!, body.Content, body.Time);
            return Ok();
        }

        [HttpPut("user/{username}/timeline/description")]
        [Authorize]
        [SelfOrAdmin]
        public async Task<ActionResult> TimelinePutDescription([FromRoute][Username] string username, [FromBody] string body)
        {
            await _service.SetDescription(username, body);
            return Ok();
        }

        private static TimelineVisibility StringToVisibility(string s)
        {
            if ("public".Equals(s, StringComparison.InvariantCultureIgnoreCase))
            {
                return TimelineVisibility.Public;
            }
            else if ("register".Equals(s, StringComparison.InvariantCultureIgnoreCase))
            {
                return TimelineVisibility.Register;
            }
            else if ("private".Equals(s, StringComparison.InvariantCultureIgnoreCase))
            {
                return TimelineVisibility.Private;
            }
            throw new ArgumentException(ExceptionStringToVisibility);
        }

        [HttpPut("user/{username}/timeline/visibility")]
        [Authorize]
        [SelfOrAdmin]
        public async Task<ActionResult> TimelinePutVisibility([FromRoute][Username] string username, [FromBody][RegularExpression("public|register|private")] string body)
        {
            await _service.SetVisibility(username, StringToVisibility(body));
            return Ok();
        }

        [HttpPost("user/{username}/timeline/members/change")]
        [Authorize]
        [SelfOrAdmin]
        public async Task<ActionResult> TimelineMembersChange([FromRoute][Username] string username, [FromBody] TimelineMemberChangeRequest body)
        {
            //TODO!
        }
    }
}
