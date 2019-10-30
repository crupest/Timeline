using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Threading.Tasks;
using Timeline.Auth;
using Timeline.Helpers;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Models.Validation;
using Timeline.Services;
using static Timeline.Resources.Controllers.UserController;

namespace Timeline
{
    public static partial class ErrorCodes
    {
        public static partial class Http
        {
            public static class User // bbb = 002
            {
                public static class Get // cc = 01
                {
                    public const int NotExist = 10020101; // dd = 01
                }

                public static class Patch // cc = 03
                {
                    public const int NotExist = 10020301; // dd = 01
                }

                public static class Op // cc = 1x
                {
                    public static class ChangeUsername // cc = 11
                    {
                        public const int NotExist = 10021101; // dd = 01
                        public const int AlreadyExist = 10021102; // dd = 02
                    }

                    public static class ChangePassword // cc = 12
                    {
                        public const int BadOldPassword = 10021201; // dd = 01
                    }
                }

            }
        }
    }
}

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

        [HttpGet("users"), AdminAuthorize]
        public async Task<ActionResult<UserInfo[]>> List()
        {
            return Ok(await _userService.ListUsers());
        }

        [HttpGet("users/{username}"), AdminAuthorize]
        public async Task<ActionResult<UserInfo>> Get([FromRoute][Username] string username)
        {
            var user = await _userService.GetUser(username);
            if (user == null)
            {
                _logger.LogInformation(Log.Format(LogGetUserNotExist, ("Username", username)));
                return NotFound(new CommonResponse(ErrorCodes.Http.User.Get.NotExist, ErrorGetUserNotExist));
            }
            return Ok(user);
        }

        [HttpPut("users/{username}"), AdminAuthorize]
        public async Task<ActionResult<CommonPutResponse>> Put([FromBody] UserPutRequest request, [FromRoute][Username] string username)
        {
            var result = await _userService.PutUser(username, request.Password, request.Administrator!.Value);
            switch (result)
            {
                case PutResult.Create:
                    _logger.LogInformation(Log.Format(LogPutCreate, ("Username", username)));
                    return CreatedAtAction("Get", new { username }, CommonPutResponse.Create());
                case PutResult.Modify:
                    _logger.LogInformation(Log.Format(LogPutModify, ("Username", username)));
                    return Ok(CommonPutResponse.Modify());
                default:
                    throw new InvalidBranchException();
            }
        }

        [HttpPatch("users/{username}"), AdminAuthorize]
        public async Task<ActionResult> Patch([FromBody] UserPatchRequest request, [FromRoute][Username] string username)
        {
            try
            {
                await _userService.PatchUser(username, request.Password, request.Administrator);
                return Ok();
            }
            catch (UserNotExistException e)
            {
                _logger.LogInformation(e, Log.Format(LogPatchUserNotExist, ("Username", username)));
                return NotFound(new CommonResponse(ErrorCodes.Http.User.Patch.NotExist, ErrorPatchUserNotExist));
            }
        }

        [HttpDelete("users/{username}"), AdminAuthorize]
        public async Task<ActionResult<CommonDeleteResponse>> Delete([FromRoute][Username] string username)
        {
            try
            {
                await _userService.DeleteUser(username);
                _logger.LogInformation(Log.Format(LogDeleteDelete, ("Username", username)));
                return Ok(CommonDeleteResponse.Delete());
            }
            catch (UserNotExistException e)
            {
                _logger.LogInformation(e, Log.Format(LogDeleteNotExist, ("Username", username)));
                return Ok(CommonDeleteResponse.NotExist());
            }
        }

        [HttpPost("userop/changeusername"), AdminAuthorize]
        public async Task<ActionResult> ChangeUsername([FromBody] ChangeUsernameRequest request)
        {
            try
            {
                await _userService.ChangeUsername(request.OldUsername, request.NewUsername);
                _logger.LogInformation(Log.Format(LogChangeUsernameSuccess,
                    ("Old Username", request.OldUsername), ("New Username", request.NewUsername)));
                return Ok();
            }
            catch (UserNotExistException e)
            {
                _logger.LogInformation(e, Log.Format(LogChangeUsernameNotExist,
                    ("Old Username", request.OldUsername), ("New Username", request.NewUsername)));
                return BadRequest(new CommonResponse(ErrorCodes.Http.User.Op.ChangeUsername.NotExist,
                    string.Format(CultureInfo.CurrentCulture, ErrorChangeUsernameNotExist, request.OldUsername)));
            }
            catch (UsernameConfictException e)
            {
                _logger.LogInformation(e, Log.Format(LogChangeUsernameAlreadyExist,
                    ("Old Username", request.OldUsername), ("New Username", request.NewUsername)));
                return BadRequest(new CommonResponse(ErrorCodes.Http.User.Op.ChangeUsername.AlreadyExist, ErrorChangeUsernameAlreadyExist));
            }
            // there is no need to catch bad format exception because it is already checked in model validation.
        }

        [HttpPost("userop/changepassword"), Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                await _userService.ChangePassword(User.Identity.Name!, request.OldPassword, request.NewPassword);
                _logger.LogInformation(Log.Format(LogChangePasswordSuccess, ("Username", User.Identity.Name)));
                return Ok();
            }
            catch (BadPasswordException e)
            {
                _logger.LogInformation(e, Log.Format(LogChangePasswordBadPassword,
                    ("Username", User.Identity.Name), ("Old Password", request.OldPassword)));
                return BadRequest(new CommonResponse(ErrorCodes.Http.User.Op.ChangePassword.BadOldPassword,
                    ErrorChangePasswordBadPassword));
            }
            // User can't be non-existent or the token is bad. 
        }
    }
}
