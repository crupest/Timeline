using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimelineApp.Auth;

namespace TimelineApp.Controllers.Testing
{
    [Route("testing/auth")]
    [ApiController]
    public class TestingAuthController : Controller
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
