using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timeline.Auth;
using Timeline.Filters;
using Timeline.Models.Http;
using Timeline.Models.Validation;
using Timeline.Services.Mapper;
using Timeline.Services.User;

namespace Timeline.Controllers
{
    /// <summary>
    /// Operations about users.
    /// </summary>
    [ApiController]
    [ProducesErrorResponseType(typeof(CommonResponse))]
    public class UserController : MyControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserPermissionService _userPermissionService;
        private readonly IUserDeleteService _userDeleteService;
        private readonly IGenericMapper _mapper;

        /// <summary></summary>
        public UserController(IUserService userService, IUserPermissionService userPermissionService, IUserDeleteService userDeleteService, IGenericMapper mapper)
        {
            _userService = userService;
            _userPermissionService = userPermissionService;
            _userDeleteService = userDeleteService;
            _mapper = mapper;
        }

        private bool UserHasUserManagementPermission => UserHasPermission(UserPermission.UserManagement);

        /// <summary>
        /// Get all users.
        /// </summary>
        /// <returns>All user list.</returns>
        [HttpGet("users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<HttpUser>>> List()
        {
            var users = await _userService.GetUsersAsync();
            var result = await _mapper.MapListAsync<HttpUser>(users, Url, User);
            return result;
        }

        /// <summary>
        /// Create a new user. You have to be administrator.
        /// </summary>
        /// <returns>The new user's info.</returns>
        [HttpPost("users")]
        [PermissionAuthorize(UserPermission.UserManagement)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<HttpUser>> Post([FromBody] HttpUserPostRequest body)
        {
            var user = await _userService.CreateUserAsync(
                new CreateUserParams(body.Username, body.Password) { Nickname = body.Nickname });
            return await _mapper.MapAsync<HttpUser>(user, Url, User);
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
            var id = await _userService.GetUserIdByUsernameAsync(username);
            var user = await _userService.GetUserAsync(id);
            return await _mapper.MapAsync<HttpUser>(user, Url, User);
        }

        /// <summary>
        /// Change a user's property.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="username">Username of the user to change.</param>
        /// <returns>The new user info.</returns>
        [HttpPatch("users/{username}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HttpUser>> Patch([FromBody] HttpUserPatchRequest body, [FromRoute][Username] string username)
        {
            if (UserHasUserManagementPermission)
            {
                var id = await _userService.GetUserIdByUsernameAsync(username);
                var user = await _userService.ModifyUserAsync(id, _mapper.AutoMapperMap<ModifyUserParams>(body));
                return await _mapper.MapAsync<HttpUser>(user, Url, User);
            }
            else
            {
                if (GetUsername() != username)
                    return ForbidWithCommonResponse(Resource.MessageForbidNotAdministratorOrOwner);

                if (body.Username is not null)
                    return ForbidWithCommonResponse(Resource.MessageForbidNotAdministrator);

                if (body.Password is not null)
                    return ForbidWithCommonResponse(Resource.MessageForbidNotAdministrator);

                var user = await _userService.ModifyUserAsync(GetUserId(), _mapper.AutoMapperMap<ModifyUserParams>(body));
                return await _mapper.MapAsync<HttpUser>(user, Url, User);
            }
        }

        /// <summary>
        /// Delete a user and all his related data. You have to be administrator.
        /// </summary>
        /// <param name="username">Username of the user to delete.</param>
        /// <returns>Info of deletion.</returns>
        [HttpDelete("users/{username}")]
        [PermissionAuthorize(UserPermission.UserManagement)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CommonDeleteResponse>> Delete([FromRoute][Username] string username)
        {
            try
            {
                await _userDeleteService.DeleteUserAsync(username);
                return DeleteWithCommonDeleteResponse();
            }
            catch (InvalidOperationOnRootUserException)
            {
                return BadRequestWithCommonResponse(ErrorCodes.UserController.InvalidOperationOnRootUser, Resource.MessageInvalidOperationOnRootUser);
            }
        }

        /// <summary>
        /// Change password with old password.
        /// </summary>
        [HttpPost("userop/changepassword"), Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<CommonResponse>> ChangePassword([FromBody] HttpChangePasswordRequest request)
        {
            try
            {
                await _userService.ChangePassword(GetUserId(), request.OldPassword, request.NewPassword);
                return OkWithCommonResponse();
            }
            catch (BadPasswordException)
            {
                return BadRequestWithCommonResponse(ErrorCodes.UserController.ChangePasswordBadOldPassword, Resource.MessageOldPasswordWrong);
            }
            // User can't be non-existent or the token is bad.
        }

        [HttpPut("users/{username}/permissions/{permission}"), PermissionAuthorize(UserPermission.UserManagement)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommonResponse>> PutUserPermission([FromRoute][Username] string username, [FromRoute] UserPermission permission)
        {
            try
            {
                var id = await _userService.GetUserIdByUsernameAsync(username);
                await _userPermissionService.AddPermissionToUserAsync(id, permission);
                return OkWithCommonResponse();
            }
            catch (InvalidOperationOnRootUserException)
            {
                return BadRequestWithCommonResponse(ErrorCodes.UserController.InvalidOperationOnRootUser, Resource.MessageInvalidOperationOnRootUser);
            }
        }

        [HttpDelete("users/{username}/permissions/{permission}"), PermissionAuthorize(UserPermission.UserManagement)]
        [NotEntityDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CommonResponse>> DeleteUserPermission([FromRoute][Username] string username, [FromRoute] UserPermission permission)
        {
            try
            {
                var id = await _userService.GetUserIdByUsernameAsync(username);
                await _userPermissionService.RemovePermissionFromUserAsync(id, permission);
                return OkWithCommonResponse();
            }
            catch (InvalidOperationOnRootUserException)
            {
                return BadRequestWithCommonResponse(ErrorCodes.UserController.InvalidOperationOnRootUser, Resource.MessageInvalidOperationOnRootUser);
            }
        }
    }
}
