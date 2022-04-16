using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Timeline.Filters;
using Timeline.Helpers.Cache;
using Timeline.Models;
using Timeline.Models.Validation;
using Timeline.Services.User;
using Timeline.Services.User.Avatar;

namespace Timeline.Controllers.V2
{
    /// <summary>
    /// Operations about user avatar.
    /// </summary>
    [ApiController]
    [Route("v2/users/{username}/avatar")]
    public class UserAvatarV2Controller : V2ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserAvatarService _service;

        public UserAvatarV2Controller(IUserService userService, IUserAvatarService service)
        {
            _userService = userService;
            _service = service;
        }

        /// <summary>
        /// Get avatar of a user.
        /// </summary>
        /// <param name="username">Username of the user to get avatar of.</param>
        /// <param name="ifNoneMatch">If-None-Match header.</param>
        /// <returns>Avatar data.</returns>
        [HttpGet]
        [ProducesImages]
        [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([FromRoute][Username] string username, [FromHeader(Name = "If-None-Match")] string? ifNoneMatch)
        {
            _ = ifNoneMatch;
            long userId = await _userService.GetUserIdByUsernameAsync(username);
            return await DataCacheHelper.GenerateActionResult(this, () => _service.GetAvatarDigestAsync(userId), () => _service.GetAvatarAsync(userId));
        }

        /// <summary>
        /// Set avatar of a user. You have to be administrator to change other's.
        /// </summary>
        /// <param name="username">Username of the user to set avatar of.</param>
        /// <param name="body">The avatar data.</param>
        [HttpPut]
        [Authorize]
        [ConsumesImages]
        [MaxContentLength(1000 * 1000 * 10)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Put([FromRoute][Username] string username, [FromBody] ByteData body)
        {
            long userId = await _userService.GetUserIdByUsernameAsync(username);

            if (!UserHasPermission(UserPermission.UserManagement) && GetAuthUserId() != userId)
            {
                return Forbid();
            }


            var digest = await _service.SetAvatarAsync(userId, body);

            Response.Headers.Append("ETag", $"\"{digest.ETag}\"");
            return NoContent();
        }

        /// <summary>
        /// Reset the avatar to the default one. You have to be administrator to reset other's.
        /// </summary>
        /// <param name="username">Username of the user.</param>
        /// <response code="200">Succeeded to reset.</response>
        /// <response code="401">You have not logged in.</response>
        /// <response code="403">You are not administrator.</response>
        [HttpDelete]
        [Authorize]
        [NotEntityDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Delete([FromRoute][Username] string username)
        {
            long userId = await _userService.GetUserIdByUsernameAsync(username);

            if (!UserHasPermission(UserPermission.UserManagement) && GetAuthUserId() != userId)
            {
                return Forbid();
            }

            await _service.DeleteAvatarAsync(userId);
            return NoContent();
        }
    }
}
