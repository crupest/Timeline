using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Timeline.Models.Http;
using Timeline.Services.User;

namespace Timeline.Controllers.V2
{
    [ApiController]
    [Route("v2/self")]
    public class SelfController : V2ControllerBase
    {
        private readonly IUserService _userService;

        public SelfController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("changepassword")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> ChangePasswordAsync([FromBody] HttpChangePasswordRequest body)
        {
            try
            {
                await _userService.ChangePassword(GetAuthUserId(), body.OldPassword, body.NewPassword);
                return NoContent();
            }
            catch (BadPasswordException)
            {
                return UnprocessableEntity(new ErrorResponse(ErrorResponse.InvalidRequest, "Old password is wrong."));
            }
        }
    }
}

