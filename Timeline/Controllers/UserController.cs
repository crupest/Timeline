using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Timeline.Authenticate;
using Timeline.Helpers;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Services;

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

                public static class Put // cc = 02
                {
                    public const int BadUsername = 10020201; // dd = 01
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
        private readonly IStringLocalizerFactory _localizerFactory;
        private readonly IStringLocalizer _localizer;

        public UserController(ILogger<UserController> logger, IUserService userService, IStringLocalizerFactory localizerFactory)
        {
            _logger = logger;
            _userService = userService;
            _localizerFactory = localizerFactory;
            _localizer = localizerFactory.Create(GetType());
        }

        [HttpGet("users"), AdminAuthorize]
        public async Task<ActionResult<UserInfo[]>> List()
        {
            return Ok(await _userService.ListUsers());
        }

        [HttpGet("users/{username}"), AdminAuthorize]
        public async Task<ActionResult<UserInfo>> Get([FromRoute] string username)
        {
            var user = await _userService.GetUser(username);
            if (user == null)
            {
                _logger.LogInformation(Log.Format(_localizer["LogGetUserNotExist"], ("Username", username)));
                return NotFound(new CommonResponse(ErrorCodes.Http.User.Get.NotExist, _localizer["ErrorGetUserNotExist"]));
            }
            return Ok(user);
        }

        [HttpPut("users/{username}"), AdminAuthorize]
        public async Task<ActionResult<CommonPutResponse>> Put([FromBody] UserPutRequest request, [FromRoute] string username)
        {
            try
            {
                var result = await _userService.PutUser(username, request.Password, request.Administrator!.Value);
                switch (result)
                {
                    case PutResult.Created:
                        _logger.LogInformation(Log.Format(_localizer["LogPutCreate"], ("Username", username)));
                        return CreatedAtAction("Get", new { username }, CommonPutResponse.Create(_localizerFactory));
                    case PutResult.Modified:
                        _logger.LogInformation(Log.Format(_localizer["LogPutModify"], ("Username", username)));
                        return Ok(CommonPutResponse.Modify(_localizerFactory));
                    default:
                        throw new InvalidBranchException();
                }
            }
            catch (UsernameBadFormatException e)
            {
                _logger.LogInformation(e, Log.Format(_localizer["LogPutBadUsername"], ("Username", username)));
                return BadRequest(new CommonResponse(ErrorCodes.Http.User.Put.BadUsername, _localizer["ErrorPutBadUsername"]));
            }
        }

        [HttpPatch("users/{username}"), AdminAuthorize]
        public async Task<ActionResult> Patch([FromBody] UserPatchRequest request, [FromRoute] string username)
        {
            try
            {
                await _userService.PatchUser(username, request.Password, request.Administrator);
                return Ok();
            }
            catch (UserNotExistException e)
            {
                _logger.LogInformation(e, Log.Format(_localizer["LogPatchUserNotExist"], ("Username", username)));
                return NotFound(new CommonResponse(ErrorCodes.Http.User.Patch.NotExist, _localizer["ErrorPatchUserNotExist"]));
            }
        }

        [HttpDelete("users/{username}"), AdminAuthorize]
        public async Task<ActionResult<CommonDeleteResponse>> Delete([FromRoute] string username)
        {
            try
            {
                await _userService.DeleteUser(username);
                _logger.LogInformation(Log.Format(_localizer["LogDeleteDelete"], ("Username", username)));
                return Ok(CommonDeleteResponse.Delete(_localizerFactory));
            }
            catch (UserNotExistException e)
            {
                _logger.LogInformation(e, Log.Format(_localizer["LogDeleteUserNotExist"], ("Username", username)));
                return Ok(CommonDeleteResponse.NotExist(_localizerFactory));
            }
        }

        [HttpPost("userop/changeusername"), AdminAuthorize]
        public async Task<ActionResult> ChangeUsername([FromBody] ChangeUsernameRequest request)
        {
            try
            {
                await _userService.ChangeUsername(request.OldUsername, request.NewUsername);
                _logger.LogInformation(Log.Format(_localizer["LogChangeUsernameSuccess"],
                    ("Old Username", request.OldUsername), ("New Username", request.NewUsername)));
                return Ok();
            }
            catch (UserNotExistException e)
            {
                _logger.LogInformation(e, Log.Format(_localizer["LogChangeUsernameNotExist"],
                    ("Old Username", request.OldUsername), ("New Username", request.NewUsername)));
                return BadRequest(new CommonResponse(ErrorCodes.Http.User.Op.ChangeUsername.NotExist, _localizer["ErrorChangeUsernameNotExist", request.OldUsername]));
            }
            catch (UserAlreadyExistException e)
            {
                _logger.LogInformation(e, Log.Format(_localizer["LogChangeUsernameAlreadyExist"],
                    ("Old Username", request.OldUsername), ("New Username", request.NewUsername)));
                return BadRequest(new CommonResponse(ErrorCodes.Http.User.Op.ChangeUsername.AlreadyExist, _localizer["ErrorChangeUsernameAlreadyExist"]));
            }
            // there is no need to catch bad format exception because it is already checked in model validation.
        }

        [HttpPost("userop/changepassword"), Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                await _userService.ChangePassword(User.Identity.Name!, request.OldPassword, request.NewPassword);
                _logger.LogInformation(Log.Format(_localizer["LogChangePasswordSuccess"], ("Username", User.Identity.Name)));
                return Ok();
            }
            catch (BadPasswordException e)
            {
                _logger.LogInformation(e, Log.Format(_localizer["LogChangePasswordBadPassword"],
                    ("Username", User.Identity.Name), ("Old Password", request.OldPassword)));
                return BadRequest(new CommonResponse(ErrorCodes.Http.User.Op.ChangePassword.BadOldPassword,
                    _localizer["ErrorChangePasswordBadPassword"]));
            }
            // User can't be non-existent or the token is bad. 
        }
    }
}
