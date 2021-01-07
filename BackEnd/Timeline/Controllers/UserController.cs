using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Timeline.Auth;
using Timeline.Helpers;
using Timeline.Models.Http;
using Timeline.Models.Mapper;
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
        private readonly IUserCredentialService _userCredentialService;
        private readonly IUserPermissionService _userPermissionService;
        private readonly IUserDeleteService _userDeleteService;
        private readonly IMapper _mapper;

        /// <summary></summary>
        public UserController(ILogger<UserController> logger, IUserService userService, IUserCredentialService userCredentialService, IUserPermissionService userPermissionService, IUserDeleteService userDeleteService, IMapper mapper)
        {
            _logger = logger;
            _userService = userService;
            _userCredentialService = userCredentialService;
            _userPermissionService = userPermissionService;
            _userDeleteService = userDeleteService;
            _mapper = mapper;
        }

        private bool UserHasUserManagementPermission => this.UserHasPermission(UserPermission.UserManagement);

        /// <summary>
        /// Get all users.
        /// </summary>
        /// <returns>All user list.</returns>
        [HttpGet("users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<HttpUser[]>> List()
        {
            var users = await _userService.GetUsers();
            var result = users.MapToHttp(Url);
            return Ok(result);
        }

        /// <summary>
        /// Get a user's info.
        /// </summary>
        /// <param name="username">Username of the user.</param>
        /// <returns>User info.</returns>
        [HttpGet("users/{username}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HttpUser>> Get([FromRoute][Username] string username)
        {
            try
            {
                var id = await _userService.GetUserIdByUsername(username);
                var user = await _userService.GetUser(id);
                return Ok(user.MapToHttp(Url));
            }
            catch (UserNotExistException e)
            {
                _logger.LogInformation(e, Log.Format(LogGetUserNotExist, ("Username", username)));
                return NotFound(ErrorResponse.UserCommon.NotExist());
            }
        }

        /// <summary>
        /// Change a user's property.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="username">Username of the user to change.</param>
        /// <returns>The new user info.</returns>
        [HttpPatch("users/{username}"), Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HttpUser>> Patch([FromBody] HttpUserPatchRequest body, [FromRoute][Username] string username)
        {
            if (UserHasUserManagementPermission)
            {
                try
                {
                    var id = await _userService.GetUserIdByUsername(username);
                    var user = await _userService.ModifyUser(id, _mapper.Map<ModifyUserParams>(body));
                    return Ok(user.MapToHttp(Url));
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
                if (User.Identity!.Name != username)
                    return StatusCode(StatusCodes.Status403Forbidden,
                        ErrorResponse.Common.CustomMessage_Forbid(Common_Forbid_NotSelf));

                if (body.Username != null)
                    return StatusCode(StatusCodes.Status403Forbidden,
                        ErrorResponse.Common.CustomMessage_Forbid(UserController_Patch_Forbid_Username));

                if (body.Password != null)
                    return StatusCode(StatusCodes.Status403Forbidden,
                        ErrorResponse.Common.CustomMessage_Forbid(UserController_Patch_Forbid_Password));

                var user = await _userService.ModifyUser(this.GetUserId(), _mapper.Map<ModifyUserParams>(body));
                return Ok(user.MapToHttp(Url));
            }
        }

        /// <summary>
        /// Delete a user and all his related data. You have to be administrator.
        /// </summary>
        /// <param name="username">Username of the user to delete.</param>
        /// <returns>Info of deletion.</returns>
        [HttpDelete("users/{username}"), PermissionAuthorize(UserPermission.UserManagement)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CommonDeleteResponse>> Delete([FromRoute][Username] string username)
        {
            try
            {
                var delete = await _userDeleteService.DeleteUser(username);
                if (delete)
                    return Ok(CommonDeleteResponse.Delete());
                else
                    return Ok(CommonDeleteResponse.NotExist());
            }
            catch (InvalidOperationOnRootUserException)
            {
                return BadRequest(ErrorResponse.UserController.Delete_RootUser());
            }
        }

        /// <summary>
        /// Create a new user. You have to be administrator.
        /// </summary>
        /// <returns>The new user's info.</returns>
        [HttpPost("userop/createuser"), PermissionAuthorize(UserPermission.UserManagement)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<HttpUser>> CreateUser([FromBody] HttpCreateUserRequest body)
        {
            try
            {
                var user = await _userService.CreateUser(body.Username, body.Password);
                return Ok(user.MapToHttp(Url));
            }
            catch (EntityAlreadyExistException e) when (e.EntityName == EntityNames.User)
            {
                return BadRequest(ErrorResponse.UserController.UsernameConflict());
            }
        }

        /// <summary>
        /// Change password with old password.
        /// </summary>
        [HttpPost("userop/changepassword"), Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> ChangePassword([FromBody] HttpChangePasswordRequest request)
        {
            try
            {
                await _userCredentialService.ChangePassword(this.GetUserId(), request.OldPassword, request.NewPassword);
                return Ok();
            }
            catch (BadPasswordException e)
            {
                _logger.LogInformation(e, Log.Format(LogChangePasswordBadPassword,
                    ("Username", User.Identity!.Name), ("Old Password", request.OldPassword)));
                return BadRequest(ErrorResponse.UserController.ChangePassword_BadOldPassword());
            }
            // User can't be non-existent or the token is bad.
        }

        [HttpPut("users/{username}/permissions/{permission}"), PermissionAuthorize(UserPermission.UserManagement)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> PutUserPermission([FromRoute][Username] string username, [FromRoute] UserPermission permission)
        {
            try
            {
                var id = await _userService.GetUserIdByUsername(username);
                await _userPermissionService.AddPermissionToUserAsync(id, permission);
                return Ok();
            }
            catch (UserNotExistException)
            {
                return NotFound(ErrorResponse.UserCommon.NotExist());
            }
            catch (InvalidOperationOnRootUserException)
            {
                return BadRequest(ErrorResponse.UserController.ChangePermission_RootUser());
            }
        }

        [HttpDelete("users/{username}/permissions/{permission}"), PermissionAuthorize(UserPermission.UserManagement)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteUserPermission([FromRoute][Username] string username, [FromRoute] UserPermission permission)
        {
            try
            {
                var id = await _userService.GetUserIdByUsername(username);
                await _userPermissionService.RemovePermissionFromUserAsync(id, permission);
                return Ok();
            }
            catch (UserNotExistException)
            {
                return NotFound(ErrorResponse.UserCommon.NotExist());
            }
            catch (InvalidOperationOnRootUserException)
            {
                return BadRequest(ErrorResponse.UserController.ChangePermission_RootUser());
            }
        }
    }
}
