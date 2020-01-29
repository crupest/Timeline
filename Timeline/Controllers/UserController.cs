using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Auth;
using Timeline.Helpers;
using Timeline.Models.Http;
using Timeline.Models.Validation;
using Timeline.Services;
using static Timeline.Resources.Controllers.UserController;
using static Timeline.Resources.Messages;

namespace Timeline.Controllers
{
    [ApiController]
    public class UserController : Controller
    {

        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;

        public UserController(ILogger<UserController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [HttpGet("users")]
        public async Task<ActionResult<User[]>> List()
        {
            var users = await _userService.GetUsers();
            return Ok(users.Select(u => u.EraseSecretAndFinalFill(Url, this.IsAdministrator())).ToArray());
        }

        [HttpGet("users/{username}")]
        public async Task<ActionResult<User>> Get([FromRoute][Username] string username)
        {
            try
            {
                var user = await _userService.GetUserByUsername(username);
                return Ok(user.EraseSecretAndFinalFill(Url, this.IsAdministrator()));
            }
            catch (UserNotExistException e)
            {
                _logger.LogInformation(e, Log.Format(LogGetUserNotExist, ("Username", username)));
                return NotFound(ErrorResponse.UserCommon.NotExist());
            }
        }

        [HttpPatch("users/{username}"), Authorize]
        public async Task<ActionResult> Patch([FromBody] UserPatchRequest body, [FromRoute][Username] string username)
        {
            static User Convert(UserPatchRequest body)
            {
                return new User
                {
                    Username = body.Username,
                    Password = body.Password,
                    Administrator = body.Administrator,
                    Nickname = body.Nickname
                };
            }

            if (this.IsAdministrator())
            {
                try
                {
                    await _userService.ModifyUser(username, Convert(body));
                    return Ok();
                }
                catch (UserNotExistException e)
                {
                    _logger.LogInformation(e, Log.Format(LogPatchUserNotExist, ("Username", username)));
                    return NotFound(ErrorResponse.UserCommon.NotExist());
                }
            }
            else
            {
                if (User.Identity.Name != username)
                    return StatusCode(StatusCodes.Status403Forbidden,
                        ErrorResponse.Common.CustomMessage_Forbid(Common_Forbid_NotSelf));

                if (body.Username != null)
                    return StatusCode(StatusCodes.Status403Forbidden,
                        ErrorResponse.Common.CustomMessage_Forbid(UserController_Patch_Forbid_Username));

                if (body.Password != null)
                    return StatusCode(StatusCodes.Status403Forbidden,
                        ErrorResponse.Common.CustomMessage_Forbid(UserController_Patch_Forbid_Password));

                if (body.Administrator != null)
                    return StatusCode(StatusCodes.Status403Forbidden,
                        ErrorResponse.Common.CustomMessage_Forbid(UserController_Patch_Forbid_Administrator));

                await _userService.ModifyUser(this.GetUserId(), Convert(body));
                return Ok();
            }
        }

        [HttpDelete("users/{username}"), AdminAuthorize]
        public async Task<ActionResult<CommonDeleteResponse>> Delete([FromRoute][Username] string username)
        {
            try
            {
                await _userService.DeleteUser(username);
                return Ok(CommonDeleteResponse.Delete());
            }
            catch (UserNotExistException)
            {
                return Ok(CommonDeleteResponse.NotExist());
            }
        }

        [HttpPost("userop/create"), AdminAuthorize]
        public async Task<ActionResult> CreateUser([FromBody] User body)
        {

        }

        [HttpPost("userop/changepassword"), Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                await _userService.ChangePassword(this.GetUserId(), request.OldPassword, request.NewPassword);
                return Ok();
            }
            catch (BadPasswordException e)
            {
                _logger.LogInformation(e, Log.Format(LogChangePasswordBadPassword,
                    ("Username", User.Identity.Name), ("Old Password", request.OldPassword)));
                return BadRequest(ErrorResponse.UserController.ChangePassword_BadOldPassword());
            }
            // User can't be non-existent or the token is bad. 
        }
    }
}
