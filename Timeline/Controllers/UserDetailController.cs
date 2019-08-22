using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Timeline.Authenticate;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Services;

namespace Timeline.Controllers
{
    [Route("users/{username}/details")]
    [ProducesErrorResponseType(typeof(CommonResponse))]
    [ApiController]
    public class UserDetailController : Controller
    {
        public static class ErrorCodes
        {
            public const int Get_UserNotExist = -1001;

            public const int Patch_Forbid = -2001;
            public const int Patch_UserNotExist = -2002;

        }

        private readonly ILogger<UserDetailController> _logger;
        private readonly IUserDetailService _service;

        public UserDetailController(ILogger<UserDetailController> logger, IUserDetailService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpGet()]
        [UserAuthorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDetail))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([FromRoute] string username)
        {
            try
            {
                var detail = await _service.GetUserDetail(username);
                return Ok(detail);
            }
            catch (UserNotExistException)
            {
                return NotFound(new CommonResponse(ErrorCodes.Get_UserNotExist, "The user does not exist."));
            }
        }

        [HttpPatch()]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(void))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Patch([FromRoute] string username, [FromBody] UserDetail detail)
        {
            if (!User.IsAdmin() && User.Identity.Name != username)
                return StatusCode(StatusCodes.Status403Forbidden, new CommonResponse(ErrorCodes.Patch_Forbid, "You can't change other's details unless you are admin."));

            try
            {
                await _service.UpdateUserDetail(username, detail);
                return Ok();
            }
            catch (UserNotExistException)
            {
                return NotFound(new CommonResponse(ErrorCodes.Patch_UserNotExist, "The user does not exist."));
            }
        }
    }
}
