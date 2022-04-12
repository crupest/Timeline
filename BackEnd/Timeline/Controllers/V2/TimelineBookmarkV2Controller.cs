using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Models.Validation;
using Timeline.Services;
using Timeline.Services.Api;
using Timeline.Services.Timeline;
using Timeline.Services.User;

namespace Timeline.Controllers.V2
{
    [ApiController]
    [Route("v2/users/{username}/bookmarks")]
    public class TimelineBookmarkV2Controller : V2ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITimelineService _timelineService;
        private readonly ITimelineBookmarkService1 _timelineBookmarkService;

        public TimelineBookmarkV2Controller(IUserService userService, ITimelineService timelineService, ITimelineBookmarkService1 timelineBookmarkService)
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
        public async Task<ActionResult<Page<TimelineBookmark>>> ListAsync([FromRoute][Username] string username,
                                                                          [FromQuery][PositiveInteger] int? page, [FromQuery][PositiveInteger] int? pageSize)
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
        public async Task<ActionResult<TimelineBookmark>> GetAsync([FromRoute][Username] string username, [FromRoute][PositiveInteger] int index)
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
        [HttpPost]
        public async Task<ActionResult<TimelineBookmark>> CreateAsync([FromRoute][Username] string username, [FromBody] HttpTimelineBookmarkCreateRequest body)
        {
            var userId = await _userService.GetUserIdByUsernameAsync(username);
            if (!UserHasPermission(UserPermission.UserBookmarkManagement) && GetAuthUserId() != userId)
            {
                return Forbid();
            }
            long timelineId;
            try
            {
                timelineId = await _timelineService.GetTimelineIdAsync(body.TimelineOwner, body.TimelineName);
            }
            catch (EntityNotExistException)
            {
                return UnprocessableEntity();
            }
            var bookmark = await _timelineBookmarkService.AddBookmarkAsync(userId, timelineId, body.Position);
            return CreatedAtAction("Get", new { username, index = bookmark.Position }, bookmark);
        }

        [Authorize]
        [HttpPost("delete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> DeleteAsync([FromRoute][Username] string username, [FromBody] HttpTimelinebookmarkDeleteRequest body)
        {
            var userId = await _userService.GetUserIdByUsernameAsync(username);
            if (!UserHasPermission(UserPermission.UserBookmarkManagement) && GetAuthUserId() != userId)
            {
                return Forbid();
            }

            long timelineId;
            try
            {
                timelineId = await _timelineService.GetTimelineIdAsync(body.TimelineOwner, body.TimelineName);
            }
            catch (EntityNotExistException)
            {
                return UnprocessableEntity();
            }

            await _timelineBookmarkService.DeleteBookmarkAsync(userId, timelineId);

            return NoContent();
        }

        [Authorize]
        [HttpPost("move")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<TimelineBookmark>> MoveAsync([FromRoute][Username] string username, [FromBody] HttpTimelineBookmarkMoveRequest body)
        {
            var userId = await _userService.GetUserIdByUsernameAsync(username);
            if (!UserHasPermission(UserPermission.UserBookmarkManagement) && GetAuthUserId() != userId)
            {
                return Forbid();
            }

            long timelineId;
            try
            {
                timelineId = await _timelineService.GetTimelineIdAsync(body.TimelineOwner, body.TimelineName);
            }
            catch (EntityNotExistException)
            {
                return UnprocessableEntity();
            }

            var bookmark = await _timelineBookmarkService.MoveBookmarkAsync(userId, timelineId, body.Position!.Value);

            return Ok(bookmark);
        }

        [HttpGet("visibility")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<HttpTimelineBookmarkVisibility>> GetVisibilityAsync([FromRoute][Username] string username)
        {
            var userId = await _userService.GetUserIdByUsernameAsync(username);
            var visibility = await _timelineBookmarkService.GetBookmarkVisibilityAsync(userId);
            return Ok(new HttpTimelineBookmarkVisibility { Visibility = visibility });
        }

        [HttpPut("visibility")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> PutVisibilityAsync([FromRoute][Username] string username, [FromBody] HttpTimelineBookmarkVisibility body)
        {
            var userId = await _userService.GetUserIdByUsernameAsync(username);
            if (!UserHasPermission(UserPermission.UserBookmarkManagement) && GetAuthUserId() != userId)
            {
                return Forbid();
            }
            await _timelineBookmarkService.SetBookmarkVisibilityAsync(userId, body.Visibility);
            return NoContent();
        }
    }
}
