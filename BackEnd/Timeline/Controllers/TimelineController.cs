using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Timeline.Filters;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Models.Mapper;
using Timeline.Models.Validation;
using Timeline.Services;
using Timeline.Services.Exceptions;

namespace Timeline.Controllers
{
    /// <summary>
    /// Operations about timeline.
    /// </summary>
    [ApiController]
    [Route("timelines")]
    [CatchTimelineNotExistException]
    [ProducesErrorResponseType(typeof(CommonResponse))]
    public class TimelineController : Controller
    {
        private readonly IUserService _userService;
        private readonly ITimelineService _service;

        private readonly TimelineMapper _timelineMapper;
        private readonly IMapper _mapper;

        /// <summary>
        /// 
        /// </summary>
        public TimelineController(IUserService userService, ITimelineService service, TimelineMapper timelineMapper, IMapper mapper)
        {
            _userService = userService;
            _service = service;
            _timelineMapper = timelineMapper;
            _mapper = mapper;
        }

        private bool UserHasAllTimelineManagementPermission => this.UserHasPermission(UserPermission.AllTimelineManagement);

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
        public async Task<ActionResult<List<HttpTimeline>>> TimelineList([FromQuery][Username] string? relate, [FromQuery][RegularExpression("(own)|(join)")] string? relateType, [FromQuery] string? visibility)
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
            var result = await _timelineMapper.MapToHttp(timelines, Url, this.GetOptionalUserId());
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
            var timelineId = await _service.GetTimelineIdByName(timeline);
            var t = await _service.GetTimeline(timelineId);
            var result = await _timelineMapper.MapToHttp(t, Url, this.GetOptionalUserId());
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
            var timelineId = await _service.GetTimelineIdByName(timeline);

            if (!UserHasAllTimelineManagementPermission && !await _service.HasManagePermission(timelineId, this.GetUserId()))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            try
            {
                await _service.ChangeProperty(timelineId, _mapper.Map<TimelineChangePropertyParams>(body));
                var t = await _service.GetTimeline(timelineId);
                var result = await _timelineMapper.MapToHttp(t, Url, this.GetOptionalUserId());
                return result;
            }
            catch (EntityAlreadyExistException)
            {
                return BadRequest(ErrorResponse.TimelineController.NameConflict());
            }
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
            var timelineId = await _service.GetTimelineIdByName(timeline);

            if (!UserHasAllTimelineManagementPermission && !(await _service.HasManagePermission(timelineId, this.GetUserId())))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            try
            {
                var userId = await _userService.GetUserIdByUsername(member);
                var create = await _service.AddMember(timelineId, userId);
                return Ok(CommonPutResponse.Create(create));
            }
            catch (UserNotExistException)
            {
                return BadRequest(ErrorResponse.UserCommon.NotExist());
            }
        }

        /// <summary>
        /// Remove a member from timeline.
        /// </summary>
        /// <param name="timeline">Timeline name.</param>
        /// <param name="member">The member's username.</param>
        [HttpDelete("{timeline}/members/{member}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> TimelineMemberDelete([FromRoute][GeneralTimelineName] string timeline, [FromRoute][Username] string member)
        {
            var timelineId = await _service.GetTimelineIdByName(timeline);

            if (!UserHasAllTimelineManagementPermission && !(await _service.HasManagePermission(timelineId, this.GetUserId())))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            try
            {
                var userId = await _userService.GetUserIdByUsername(member);
                var delete = await _service.RemoveMember(timelineId, userId);
                return Ok(CommonDeleteResponse.Create(delete));
            }
            catch (UserNotExistException)
            {
                return BadRequest(ErrorResponse.UserCommon.NotExist());
            }
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
            var userId = this.GetUserId();

            try
            {
                var timeline = await _service.CreateTimeline(body.Name, userId);
                var result = await _timelineMapper.MapToHttp(timeline, Url, this.GetOptionalUserId());
                return result;
            }
            catch (EntityAlreadyExistException e) when (e.EntityName == EntityNames.Timeline)
            {
                return BadRequest(ErrorResponse.TimelineController.NameConflict());
            }
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
            var timelineId = await _service.GetTimelineIdByName(timeline);

            if (!UserHasAllTimelineManagementPermission && !(await _service.HasManagePermission(timelineId, this.GetUserId())))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            try
            {
                await _service.DeleteTimeline(timelineId);
                return Ok();
            }
            catch (TimelineNotExistException)
            {
                return BadRequest(ErrorResponse.TimelineController.NotExist());
            }
        }
    }
}
