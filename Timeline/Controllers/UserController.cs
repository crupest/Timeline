using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Auth;
using Timeline.Helpers;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Models.Validation;
using Timeline.Services;
using Timeline.Services.Exceptions;
using static Timeline.Resources.Controllers.UserController;
using static Timeline.Resources.Messages;

namespace Timeline.Controllers
{
    /// <summary>
    /// Operations about users.
    /// </summary>
    [ApiController]
    [ProducesErrorResponseType(typeof(CommonResponse))]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;
        private readonly IUserDeleteService _userDeleteService;
        private readonly IMapper _mapper;

        /// <summary></summary>
        public UserController(ILogger<UserController> logger, IUserService userService, IUserDeleteService userDeleteService, IMapper mapper)
        {
            _logger = logger;
            _userService = userService;
            _userDeleteService = userDeleteService;
            _mapper = mapper;
        }

        private UserInfo ConvertToUserInfo(User user) => _mapper.Map<UserInfo>(user);

        /// <summary>
        /// Get all users.
        /// </summary>
        /// <response code="200">The user list.</response>
        [HttpGet("users")]
        [ProducesResponseType(typeof(UserInfo[]), StatusCodes.Status200OK)]
        public async Task<ActionResult<UserInfo[]>> List()
        {
            var users = await _userService.GetUsers();
            var result = users.Select(u => ConvertToUserInfo(u)).ToArray();
            return Ok(result);
        }

        /// <summary>
        /// Get a user info.
        /// </summary>
        /// <param name="username">Username of the user.</param>
        /// <response code="200">The user info.</response>
        [HttpGet("users/{username}")]
        [ProducesResponseType(typeof(UserInfo), StatusCodes.Status200OK)]
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

        /// <summary>
        /// Change a user's property. You have to be administrator in some condition.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="username">Username of the user to change.</param>
        /// <response code="200">Succeed to change the user and return the new user info.</response>
        /// <response code="401">You have not logged in.</response>
        /// <response code="403">You are not administrator.</response>
        /// <response code="404">The user to change does not exist.</response>
        [HttpPatch("users/{username}"), Authorize]
        [ProducesResponseType(typeof(UserInfo), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserInfo>> Patch([FromBody] UserPatchRequest body, [FromRoute][Username] string username)
        {
            if (this.IsAdministrator())
            {
                try
                {
                    var user = await _userService.ModifyUser(username, _mapper.Map<User>(body));
                    return Ok(ConvertToUserInfo(user));
                }
                catch (UserNotExistException e)
                {
                    _logger.LogInformation(e, Log.Format(LogPatchUserNotExist, ("Username", username)));
                    return NotFound(ErrorResponse.UserCommon.NotExist());
                }
                catch (EntityAlreadyExistException e) when (e.EntityName == EntityNames.User)
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

                var user = await _userService.ModifyUser(this.GetUserId(), _mapper.Map<User>(body));
                return Ok(ConvertToUserInfo(user));
            }
        }

        /// <summary>
        /// Delete a user and all his related data. You have to be administrator.
        /// </summary>
        /// <param name="username">Username of the user to delete.</param>
        /// <response code="200">Succeeded to delete or the user does not exist.</response>
        /// <response code="401">You have not logged in.</response>
        /// <response code="403">You are not administrator.</response>
        [HttpDelete("users/{username}"), AdminAuthorize]
        [ProducesResponseType(typeof(CommonDeleteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CommonDeleteResponse>> Delete([FromRoute][Username] string username)
        {
            var delete = await _userDeleteService.DeleteUser(username);
            if (delete)
                return Ok(CommonDeleteResponse.Delete());
            else
                return Ok(CommonDeleteResponse.NotExist());
        }

        /// <summary>
        /// Create a new user. You have to be administrator.
        /// </summary>
        /// <response code="200">Succeeded to create a new user and return his user info.</response>
        /// <response code="400">Error code is 11020101 if a user with given username already exists.</response>
        /// <response code="401">You have not logged in.</response>
        /// <response code="403">You are not administrator.</response>
        [HttpPost("userop/createuser"), AdminAuthorize]
        [ProducesResponseType(typeof(UserInfo), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<UserInfo>> CreateUser([FromBody] CreateUserRequest body)
        {
            try
            {
                var user = await _userService.CreateUser(_mapper.Map<User>(body));
                return Ok(ConvertToUserInfo(user));
            }
            catch (EntityAlreadyExistException e) when (e.EntityName == EntityNames.User)
            {
                return BadRequest(ErrorResponse.UserController.UsernameConflict());
            }
        }

        /// <summary>
        /// Change password with old password.
        /// </summary>
        /// <response code="200">Succeeded to change password.</response>
        /// <response code="400">Error code is 11020201 if old password is wrong.</response>
        /// <response code="401">You have not logged in.</response>
        [HttpPost("userop/changepassword"), Authorize]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
