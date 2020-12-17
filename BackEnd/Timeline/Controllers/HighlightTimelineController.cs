using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timeline.Auth;
using Timeline.Models.Http;
using Timeline.Models.Validation;
using Timeline.Services;
using Timeline.Services.Exceptions;

namespace Timeline.Controllers
{
    /// <summary>
    /// Api related to highlight timeline.
    /// </summary>
    [ApiController]
    [ProducesErrorResponseType(typeof(CommonResponse))]
    [Route("highlights")]
    public class HighlightTimelineController : Controller
    {
        private readonly IHighlightTimelineService _service;
        private readonly IMapper _mapper;

        public HighlightTimelineController(IHighlightTimelineService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all highlight timelines.
        /// </summary>
        /// <returns>Highlight timeline list.</returns>
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<ActionResult<List<HttpTimeline>>> List()
        {
            var t = await _service.GetHighlightTimelines();
            return _mapper.Map<List<HttpTimeline>>(t);
        }

        /// <summary>
        /// Add a timeline to highlight list.
        /// </summary>
        /// <param name="timeline"></param>
        [HttpPut("{timeline}")]
        [PermissionAuthorize(UserPermission.HighlightTimelineManagement)]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> Put([GeneralTimelineName] string timeline)
        {
            try
            {
                await _service.AddHighlightTimeline(timeline, this.GetUserId());
                return Ok();
            }
            catch (TimelineNotExistException)
            {
                return BadRequest(ErrorResponse.TimelineController.NotExist());
            }
        }
    }
}
