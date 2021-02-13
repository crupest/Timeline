using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Models.Http;
using Timeline.Models.Mapper;
using Timeline.Services;

namespace Timeline.Controllers
{
    /// <summary>
    /// Api related to search timelines or users.
    /// </summary>
    [ApiController]
    [ProducesErrorResponseType(typeof(CommonResponse))]
    [Route("search")]
    public class SearchController : Controller
    {
        private readonly ISearchService _service;
        private readonly TimelineMapper _timelineMapper;
        private readonly UserMapper _userMapper;

        public SearchController(ISearchService service, TimelineMapper timelineMapper, UserMapper userMapper)
        {
            _service = service;
            _timelineMapper = timelineMapper;
            _userMapper = userMapper;
        }

        private Task<List<HttpTimeline>> Map(List<TimelineEntity> timelines)
        {
            return _timelineMapper.MapToHttp(timelines, Url, this.GetOptionalUserId(), this.UserHasPermission(UserPermission.AllTimelineManagement));
        }

        /// <summary>
        /// Search timelines whose name or title contains query string case-insensitively.
        /// </summary>
        /// <param name="query">The string to contain.</param>
        /// <returns>Timelines with most related at first.</returns>
        [HttpGet("timelines")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<List<HttpTimeline>>> TimelineSearch([FromQuery(Name = "q"), Required(AllowEmptyStrings = false)] string query)
        {
            var searchResult = await _service.SearchTimeline(query);
            var timelines = searchResult.Items.Select(i => i.Item).ToList();
            return await Map(timelines);
        }

        /// <summary>
        /// Search users whose username or nick contains query string case-insensitively.
        /// </summary>
        /// <param name="query">The string to contain.</param>
        /// <returns>Users with most related at first.</returns>
        [HttpGet("users")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<List<HttpUser>>> UserSearch([FromQuery(Name = "q"), Required(AllowEmptyStrings = false)] string query)
        {
            var searchResult = await _service.SearchUser(query);
            var users = searchResult.Items.Select(i => i.Item).ToList();
            return await _userMapper.MapToHttp(users, Url);
        }
    }
}
