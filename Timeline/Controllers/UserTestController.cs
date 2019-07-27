using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Timeline.Authenticate;

namespace Timeline.Controllers
{
    [Route("Test/User")]
    public class UserTestController : Controller
    {
        [HttpGet("[action]")]
        [Authorize]
        public ActionResult Authorize()
        {
            return Ok();
        }

        [HttpGet("[action]")]
        [UserAuthorize]
        public new ActionResult User()
        {
            return Ok();
        }

        [HttpGet("[action]")]
        [AdminAuthorize]
        public ActionResult Admin()
        {
            return Ok();
        }
    }
}
