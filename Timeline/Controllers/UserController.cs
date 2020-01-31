using AutoMapper;
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
        private readonly IMapper _mapper;

        public UserController(ILogger<UserController> logger, IUserService userService, IMapper mapper)
        {
            _logger = logger;
            _userService = userService;
            _mapper = mapper;
        }

        private UserInfo ConvertToUserInfo(User user) => _mapper.Map<UserInfo>(user);

        [HttpGet("users")]
        public async Task<ActionResult<UserInfo[]>> List()
        {
            var users = await _userService.GetUsers();
            var result = users.Select(u => ConvertToUserInfo(u)).ToArray();
            return Ok(result);
        }

        [HttpGet("users/{username}")]
        public async Task<ActionResult<UserInfo>> Get([FromRoute][Username] string username)
        {
            try
            {
                var user = await _userService.GetUserByUsername(username);
                return Ok(ConvertToUserInfo(user));
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
            if (this.IsAdministrator())
            {
                try
                {
                    await _userService.ModifyUser(username, _mapper.Map<User>(body));
                    return Ok();
                }
                catch (UserNotExistException e)
                {
                    _logger.LogInformation(e, Log.Format(LogPatchUserNotExist, ("Username", username)));
                    return NotFound(ErrorResponse.UserCommon.NotExist());
                }
                catch (ConflictException)
                {
                    return BadRequest(ErrorResponse.UserController.UsernameConflict());
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

                await _userService.ModifyUser(this.GetUserId(), _mapper.Map<User>(body));
                return Ok();
            }
        }

        [HttpDelete("users/{username}"), AdminAuthorize]
        public async Task<ActionResult<CommonDeleteResponse>> Delete([FromRoute][Username] string username)
        {
            var delete = await _userService.DeleteUser(username);
            if (delete)
                return Ok(CommonDeleteResponse.Delete());
            else
                return Ok(CommonDeleteResponse.NotExist());
        }

        [HttpPost("userop/createuser"), AdminAuthorize]
        public async Task<ActionResult> CreateUser([FromBody] CreateUserRequest body)
        {
            try
            {
                await _userService.CreateUser(_mapper.Map<User>(body));
                return Ok();
            }
            catch (ConflictException)
            {
                return BadRequest(ErrorResponse.UserController.UsernameConflict());
            }
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
