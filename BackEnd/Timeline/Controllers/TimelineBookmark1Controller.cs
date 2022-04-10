using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Timeline.Models;

namespace Timeline.Controllers
{
    [ApiController]
    [Route("users/{username}/bookmarks")]
    public class TimelineBookmark1Controller : MyControllerBase
    {
        public TimelineBookmark1Controller()
        {
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<TimelineBookmark>> ListAsync([FromRoute] string username)
        {
            throw new NotImplementedException();
        }
    }
}
