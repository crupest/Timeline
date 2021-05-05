using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Filters;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Models.Validation;
using Timeline.Services;
using Timeline.Services.Mapper;
using Timeline.Services.Timeline;
using Timeline.Services.User;

namespace Timeline.Controllers
{
    /// <summary>
    /// Operations about timeline.
    /// </summary>
    [ApiController]
    [Route("timelines")]
    [ProducesErrorResponseType(typeof(CommonResponse))]
    public class TimelineController : MyControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITimelineService _service;
        private readonly IGenericMapper _mapper;

        public TimelineController(IUserService userService, ITimelineService service, IGenericMapper mapper)
        {
            _userService = userService;
            _service = service;
            _mapper = mapper;
        }

        private bool UserHasAllTimelineManagementPermission => UserHasPermission(UserPermission.AllTimelineManagement);

        private Task<HttpTimeline> Map(TimelineEntity timeline)
        {
            return _mapper.MapAsync<HttpTimeline>(timeline, Url, User);
        }

        private Task<List<HttpTimeline>> Map(List<TimelineEntity> timelines)
        {
            return _mapper.MapListAsync<HttpTimeline>(timelines, Url, User);
        }

        /// <summary>
        /// List all timelines.
        /// </summary>
        /// <param name="relate">A username. If set, only timelines related to the user will return.</param>
        /// <param name="relateType">Specify the relation type, may be 'own' or 'join'. If not set, both type will return.</param>
        /// <param name="visibility">"Private" or "Register" or "Public". If set, only timelines whose visibility is specified one will return.</param>
        /// <returns>The timeline list.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<HttpTimeline>>> TimelineList([FromQuery][Username] string? relate, [FromQuery][ValidationSet("own", "join", "default")] string? relateType, [FromQuery] string? visibility)
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
                        return BadRequestWithCommonResponse(ErrorCodes.Common.InvalidModel, string.Format(Resource.MessageTimelineListQueryVisibilityUnknown, visibility));
                    }
                }
            }

            TimelineUserRelationship? relationship = null;
            if (relate != null)
            {
                try
                {
                    var relatedUserId = await _userService.GetUserIdByUsernameAsync(relate);

                    var relationType = relateType is null ? TimelineUserRelationshipType.Default : Enum.Parse<TimelineUserRelationshipType>(relateType, true);

                    relationship = new TimelineUserRelationship(relationType, relatedUserId);
                }
                catch (EntityNotExistException)
                {
                    return BadRequestWithCommonResponse(ErrorCodes.TimelineController.QueryRelateNotExist, Resource.MessageTimelineListQueryRelateNotExist);
                }
            }

            var timelines = await _service.GetTimelinesAsync(relationship, visibilityFilter);
            var result = await Map(timelines);
            return result;
        }

        /// <summary>
        /// Get info of a timeline.
        /// </summary>
        /// <param name="timeline">The timeline name.</param>
        /// <returns>The timeline info.</returns>
        [HttpGet("{timeline}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HttpTimeline>> TimelineGet([FromRoute][GeneralTimelineName] string timeline)
        {
            var timelineId = await _service.GetTimelineIdByNameAsync(timeline);
            var t = await _service.GetTimelineAsync(timelineId);
            var result = await Map(t);
            return result;
        }

        /// <summary>
        /// Change properties of a timeline.
        /// </summary>
        /// <param name="timeline">Timeline name.</param>
        /// <param name="body"></param>
        /// <returns>The new info.</returns>
        [HttpPatch("{timeline}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<HttpTimeline>> TimelinePatch([FromRoute][GeneralTimelineName] string timeline, [FromBody] HttpTimelinePatchRequest body)
        {
            var timelineId = await _service.GetTimelineIdByNameAsync(timeline);

            if (!UserHasAllTimelineManagementPermission && !await _service.HasManagePermissionAsync(timelineId, GetUserId()))
            {
                return ForbidWithCommonResponse();
            }

            await _service.ChangePropertyAsync(timelineId, _mapper.AutoMapperMap<TimelineChangePropertyParams>(body));
            var t = await _service.GetTimelineAsync(timelineId);
            var result = await Map(t);
            return result;
        }

        /// <summary>
        /// Add a member to timeline.
        /// </summary>
        /// <param name="timeline">Timeline name.</param>
        /// <param name="member">The new member's username.</param>
        [HttpPut("{timeline}/members/{member}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CommonPutResponse>> TimelineMemberPut([FromRoute][GeneralTimelineName] string timeline, [FromRoute][Username] string member)
        {
            var timelineId = await _service.GetTimelineIdByNameAsync(timeline);

            if (!UserHasAllTimelineManagementPermission && !(await _service.HasManagePermissionAsync(timelineId, GetUserId())))
            {
                return ForbidWithCommonResponse();
            }

            var userId = await _userService.GetUserIdByUsernameAsync(member);
            await _service.AddMemberAsync(timelineId, userId);
            return OkWithCommonResponse();
        }

        /// <summary>
        /// Remove a member from timeline.
        /// </summary>
        /// <param name="timeline">Timeline name.</param>
        /// <param name="member">The member's username.</param>
        [HttpDelete("{timeline}/members/{member}")]
        [Authorize]
        [NotEntityDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> TimelineMemberDelete([FromRoute][GeneralTimelineName] string timeline, [FromRoute][Username] string member)
        {
            var timelineId = await _service.GetTimelineIdByNameAsync(timeline);

            if (!UserHasAllTimelineManagementPermission && !(await _service.HasManagePermissionAsync(timelineId, GetUserId())))
            {
                return ForbidWithCommonResponse();
            }

            var userId = await _userService.GetUserIdByUsernameAsync(member);
            await _service.RemoveMemberAsync(timelineId, userId);
            return OkWithCommonResponse();
        }

        /// <summary>
        /// Create a timeline.
        /// </summary>
        /// <param name="body"></param>
        /// <returns>Info of new timeline.</returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<HttpTimeline>> TimelineCreate([FromBody] HttpTimelineCreateRequest body)
        {
            var userId = GetUserId();

            var timeline = await _service.CreateTimelineAsync(body.Name, userId);
            var result = await Map(timeline);
            return result;
        }

        /// <summary>
        /// Delete a timeline.
        /// </summary>
        /// <param name="timeline">Timeline name.</param>
        /// <returns>Info of deletion.</returns>
        [HttpDelete("{timeline}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> TimelineDelete([FromRoute][TimelineName] string timeline)
        {
            try
            {
                var timelineId = await _service.GetTimelineIdByNameAsync(timeline);

                if (!UserHasAllTimelineManagementPermission && !(await _service.HasManagePermissionAsync(timelineId, GetUserId())))
                {
                    return ForbidWithCommonResponse();
                }

                await _service.DeleteTimelineAsync(timelineId);
                return DeleteWithCommonDeleteResponse();
            }
            catch (EntityNotExistException)
            {
                if (UserHasAllTimelineManagementPermission)
                {
                    return DeleteWithCommonDeleteResponse(false);
                }
                else
                {
                    return ForbidWithCommonResponse();
                }
            }
        }
    }
}
