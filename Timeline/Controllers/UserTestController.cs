using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Timeline.Controllers
{
    [Route("Test/User")]
    public class UserTestController : Controller
    {
        [HttpGet("[action]")]
        [Authorize]
        public ActionResult NeedAuthorize()
        {
            return Ok();
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "user,admin")]
        public ActionResult BothUserAndAdmin()
        {
            return Ok();
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "admin")]
        public ActionResult OnlyAdmin()
        {
            return Ok();
        }
    }
}
