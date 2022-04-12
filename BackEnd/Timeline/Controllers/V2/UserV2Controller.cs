using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Timeline.Auth;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Models.Validation;
using Timeline.Services;
using Timeline.Services.User;

namespace Timeline.Controllers.V2
{
    /// <summary>
    /// Operations about users.
    /// </summary>
    [ApiController]
    [Route("v2/users")]
    public class UserV2Controller : V2ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserPermissionService _userPermissionService;
        private readonly IUserDeleteService _userDeleteService;

        public UserV2Controller(IUserService userService, IUserPermissionService userPermissionService, IUserDeleteService userDeleteService)
        {
            _userService = userService;
            _userPermissionService = userPermissionService;
            _userDeleteService = userDeleteService;
        }

        /// <summary>
        /// Get all users.
        /// </summary>
        /// <returns>All user list.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Page<HttpUser>>> ListAsync([FromQuery][PositiveInteger] int? page, [FromQuery][PositiveInteger] int? pageSize)
        {
            var p = await _userService.GetUsersV2Async(page ?? 1, pageSize ?? 20);
            var items = await MapListAsync<HttpUser>(p.Items);
            return p.WithItems(items);
        }

        /// <summary>
        /// Create a new user. You have to be administrator.
        /// </summary>
        /// <returns>The new user's info.</returns>
        [HttpPost]
        [PermissionAuthorize(UserPermission.UserManagement)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<HttpUser>> PostAsync([FromBody] HttpUserPostRequest body)
        {
            var user = await _userService.CreateUserAsync(
                new CreateUserParams(body.Username, body.Password) { Nickname = body.Nickname });
            return CreatedAtAction("Get", new { username = body.Username }, await MapAsync<HttpUser>(user));
        }

        /// <summary>
        /// Get a user's info.
        /// </summary>
        /// <param name="username">Username of the user.</param>
        /// <returns>User info.</returns>
        [HttpGet("{username}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<HttpUser>> GetAsync([FromRoute][Username] string username)
        {
            var id = await _userService.GetUserIdByUsernameAsync(username);
            var user = await _userService.GetUserAsync(id);
            return await MapAsync<HttpUser>(user);
        }

        /// <summary>
        /// Change a user's property.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="username">Username of the user to change.</param>
        /// <returns>The new user info.</returns>
        [HttpPatch("{username}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<HttpUser>> PatchAsync([FromBody] HttpUserPatchRequest body, [FromRoute][Username] string username)
        {
            var userId = await _userService.GetUserIdByUsernameAsync(username);
            if (UserHasPermission(UserPermission.UserManagement))
            {
                var user = await _userService.ModifyUserAsync(userId, AutoMapperMap<ModifyUserParams>(body));
                return await MapAsync<HttpUser>(user);
            }
            else
            {
                if (userId != GetAuthUserId())
                    return Forbid();

                if (body.Username is not null)
                    return Forbid();

                if (body.Password is not null)
                    return Forbid();

                var user = await _userService.ModifyUserAsync(GetAuthUserId(), AutoMapperMap<ModifyUserParams>(body));
                return await MapAsync<HttpUser>(user);
            }
        }

        private const string RootUserInvalidOperationMessage = "Can't do this operation on root user.";

        /// <summary>
        /// Delete a user and all his related data. You have to be administrator.
        /// </summary>
        /// <param name="username">Username of the user to delete.</param>
        /// <returns>Info of deletion.</returns>
        [HttpDelete("{username}")]
        [PermissionAuthorize(UserPermission.UserManagement)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> DeleteAsync([FromRoute][Username] string username)
        {
            try
            {
                await _userDeleteService.DeleteUserAsync(username);
                return NoContent();
            }
            catch (EntityNotExistException)
            {
                return NoContent();
            }
            catch (InvalidOperationOnRootUserException)
            {
                return UnprocessableEntity(new ErrorResponse(ErrorResponse.InvalidOperation, RootUserInvalidOperationMessage));
            }
        }

        [HttpPut("{username}/permissions/{permission}"), PermissionAuthorize(UserPermission.UserManagement)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> PutUserPermissionAsync([FromRoute][Username] string username, [FromRoute] UserPermission permission)
        {
            try
            {
                var id = await _userService.GetUserIdByUsernameAsync(username);
                await _userPermissionService.AddPermissionToUserAsync(id, permission);
                return NoContent();
            }
            catch (InvalidOperationOnRootUserException)
            {
                return UnprocessableEntity(new ErrorResponse(ErrorResponse.InvalidOperation, RootUserInvalidOperationMessage));
            }
        }

        [HttpDelete("{username}/permissions/{permission}"), PermissionAuthorize(UserPermission.UserManagement)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> DeleteUserPermissionAsync([FromRoute][Username] string username, [FromRoute] UserPermission permission)
        {
            try
            {
                var id = await _userService.GetUserIdByUsernameAsync(username);
                await _userPermissionService.RemovePermissionFromUserAsync(id, permission);
                return NoContent();
            }
            catch (InvalidOperationOnRootUserException)
            {
                return UnprocessableEntity(new ErrorResponse(ErrorResponse.InvalidOperation, RootUserInvalidOperationMessage));
            }
        }
    }
}
