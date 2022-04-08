using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Timeline.Entities;
using Timeline.Models.Http;
using Timeline.Models.Validation;
using Timeline.Services.Mapper;
using Timeline.Services.Timeline;
using Timeline.Services.User;

namespace Timeline.Controllers
{
    [ApiController]
    [Route("v2/timelines")]
    public class TimelineV2Controller : MyControllerBase
    {
        private ITimelineService _timelineService;
        private IGenericMapper _mapper;
        private IUserService _userService;

        public TimelineV2Controller(ITimelineService timelineService, IGenericMapper mapper, IUserService userService)
        {
            _timelineService = timelineService;
            _mapper = mapper;
            _userService = userService;
        }

        private Task<HttpTimeline> MapAsync(TimelineEntity entity)
        {
            return _mapper.MapAsync<HttpTimeline>(entity, Url, User);
        }

        [HttpGet("{owner}/{timeline}")]
        public async Task<ActionResult<HttpTimeline>> GetAsync([FromRoute][Username] string owner, [FromRoute][TimelineName] string timeline)
        {
            var timelineId = await _timelineService.GetTimelineIdAsync(owner, timeline);
            var t = await _timelineService.GetTimelineAsync(timelineId);
            return await MapAsync(t);
        }

        [HttpPatch("{owner}/{timeline}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<HttpTimeline>> PatchAsync([FromRoute][Username] string owner, [FromRoute][TimelineName] string timeline, [FromBody] HttpTimelinePatchRequest body)
        {
            var timelineId = await _timelineService.GetTimelineIdAsync(owner, timeline);
            if (!UserHasPermission(UserPermission.AllTimelineManagement) && !await _timelineService.HasManagePermissionAsync(timelineId, GetAuthUserId()))
            {
                return Forbid();
            }
            await _timelineService.ChangePropertyAsync(timelineId, _mapper.AutoMapperMap<TimelineChangePropertyParams>(body));
            var t = await _timelineService.GetTimelineAsync(timelineId);
            return await MapAsync(t);
        }

        [HttpDelete("{owner}/{timeline}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> DeleteAsync([FromRoute][Username] string owner, [FromRoute][TimelineName] string timeline)
        {
            var timelineId = await _timelineService.GetTimelineIdAsync(owner, timeline);
            if (!UserHasPermission(UserPermission.AllTimelineManagement) && !await _timelineService.HasManagePermissionAsync(timelineId, GetAuthUserId()))
            {
                return Forbid();
            }
            await _timelineService.DeleteTimelineAsync(timelineId);
            return NoContent();
        }

        [HttpPut("{owner}/{timeline}/members/{member}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> MemberPutAsync([FromRoute][Username] string owner, [FromRoute][TimelineName] string timeline, [FromRoute][Username] string member)
        {
            var timelineId = await _timelineService.GetTimelineIdAsync(owner, timeline);
            if (!UserHasPermission(UserPermission.AllTimelineManagement) && !await _timelineService.HasManagePermissionAsync(timelineId, GetAuthUserId()))
            {
                return Forbid();
            }

            var userId = await _userService.GetUserIdByUsernameAsync(member);
            await _timelineService.AddMemberAsync(timelineId, userId);
            return NoContent();
        }

        [HttpDelete("{owner}/{timeline}/members/{member}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> MemberDeleteAsync([FromRoute][Username] string owner, [FromRoute][TimelineName] string timeline, [FromRoute][Username] string member)
        {
            var timelineId = await _timelineService.GetTimelineIdAsync(owner, timeline);
            if (!UserHasPermission(UserPermission.AllTimelineManagement) && !await _timelineService.HasManagePermissionAsync(timelineId, GetAuthUserId()))
            {
                return Forbid();
            }

            var userId = await _userService.GetUserIdByUsernameAsync(member);
            await _timelineService.RemoveMemberAsync(timelineId, userId);
            return NoContent();
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<HttpTimeline>> TimelineCreate([FromBody] HttpTimelineCreateRequest body)
        {
            var authUserId = GetAuthUserId();
            var authUser = await _userService.GetUserAsync(authUserId);
            var timeline = await _timelineService.CreateTimelineAsync(authUserId, body.Name);
            var result = await MapAsync(timeline);
            return CreatedAtAction("Get", new { owner = authUser.Username, timeline = body.Name }, result);
        }
    }
}

