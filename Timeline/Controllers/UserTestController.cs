using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Timeline.Controllers
{
    [Route("api/test/User")]
    public class UserTestController : Controller
    {
        [HttpGet("[action]")]
        [Authorize]
        public ActionResult NeedAuthorize()
        {
            return Ok();
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "User,Admin")]
        public ActionResult BothUserAndAdmin()
        {
            return Ok();
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "Admin")]
        public ActionResult OnlyAdmin()
        {
            return Ok();
        }
    }
}
