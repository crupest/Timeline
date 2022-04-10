using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Services.Api;
using Timeline.Services.Timeline;
using Timeline.Services.User;

namespace Timeline.Controllers
{
    [ApiController]
    [Route("users/{username}/bookmarks")]
    public class TimelineBookmark1Controller : MyControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITimelineService _timelineService;
        private readonly ITimelineBookmarkService1 _timelineBookmarkService;

        public TimelineBookmark1Controller(IUserService userService, ITimelineService timelineService, ITimelineBookmarkService1 timelineBookmarkService)
        {
            _userService = userService;
            _timelineService = timelineService;
            _timelineBookmarkService = timelineBookmarkService;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [HttpGet]
        public async Task<ActionResult<Page<TimelineBookmark>>> ListAsync([FromRoute] string username, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var userId = await _userService.GetUserIdByUsernameAsync(username);
            if (!UserHasPermission(UserPermission.UserBookmarkManagement) && !await _timelineBookmarkService.CanReadBookmarksAsync(userId, GetOptionalAuthUserId()))
            {
                return Forbid();
            }
            return await _timelineBookmarkService.GetBookmarksAsync(userId, page ?? 1, pageSize ?? 20);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [HttpGet("{index}")]
        public async Task<ActionResult<TimelineBookmark>> GetAsync([FromRoute] string username, [FromRoute] int index)
        {
            var userId = await _userService.GetUserIdByUsernameAsync(username);
            if (!UserHasPermission(UserPermission.UserBookmarkManagement) && !await _timelineBookmarkService.CanReadBookmarksAsync(userId, GetOptionalAuthUserId()))
            {
                return Forbid();
            }
            return await _timelineBookmarkService.GetBookmarkAtAsync(userId, index);
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Authorize]
        public async Task<ActionResult<TimelineBookmark>> CreateAsync([FromRoute] string username, [FromBody] HttpTimelineBookmarkCreateRequest body)
        {
            var userId = await _userService.GetUserIdByUsernameAsync(username);
            if (!UserHasPermission(UserPermission.UserBookmarkManagement) && GetAuthUserId() != userId)
            {
                return Forbid();
            }
            var timelineId = await _timelineService.GetTimelineIdAsync(body.TimelineOwner, body.TimelineName);
            var bookmark = await _timelineBookmarkService.AddBookmarkAsync(userId, timelineId, body.Position);
            return CreatedAtAction("Get", new { username, index = bookmark.Position }, bookmark);
        }
    }
}
