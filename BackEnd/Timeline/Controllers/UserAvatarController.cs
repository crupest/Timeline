using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Timeline.Filters;
using Timeline.Helpers.Cache;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Models.Validation;
using Timeline.Services.Imaging;
using Timeline.Services.User;
using Timeline.Services.User.Avatar;

namespace Timeline.Controllers
{
    /// <summary>
    /// Operations about user avatar.
    /// </summary>
    [ApiController]
    [ProducesErrorResponseType(typeof(CommonResponse))]
    public class UserAvatarController : Controller
    {
        private readonly IUserService _userService;
        private readonly IUserAvatarService _service;

        public UserAvatarController(IUserService userService, IUserAvatarService service)
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
        [HttpGet("users/{username}/avatar")]
        [Produces("image/png", "image/jpeg", "image/gif", "image/webp", "application/json", "text/json")]
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
        [HttpPut("users/{username}/avatar")]
        [Authorize]
        [Consumes("image/png", "image/jpeg", "image/gif", "image/webp")]
        [MaxContentLength(1000 * 1000 * 10)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Put([FromRoute][Username] string username, [FromBody] ByteData body)
        {
            if (!this.UserHasPermission(UserPermission.UserManagement) && User.Identity!.Name != username)
            {
                return this.ForbidWithMessage(Resource.MessageForbidNotAdministratorOrOwner);
            }

            long id = await _userService.GetUserIdByUsernameAsync(username);

            try
            {
                var digest = await _service.SetAvatarAsync(id, body);

                Response.Headers.Append("ETag", $"\"{digest.ETag}\"");

                return Ok();
            }
            catch (ImageException e)
            {
                return BadRequest(e.Error switch
                {
                    ImageException.ErrorReason.CantDecode => new CommonResponse(ErrorCodes.Image.CantDecode, Resource.MessageImageDecodeFailed),
                    ImageException.ErrorReason.UnmatchedFormat => new CommonResponse(ErrorCodes.Image.UnmatchedFormat, Resource.MessageImageFormatUnmatch),
                    ImageException.ErrorReason.BadSize => new CommonResponse(ErrorCodes.Image.BadSize, Resource.MessageImageBadSize),
                    _ => new CommonResponse(ErrorCodes.Image.Unknown, Resource.MessageImageUnknownError)
                });
            }
        }

        /// <summary>
        /// Reset the avatar to the default one. You have to be administrator to reset other's.
        /// </summary>
        /// <param name="username">Username of the user.</param>
        /// <response code="200">Succeeded to reset.</response>
        /// <response code="400">Error code is 10010001 if user does not exist.</response>
        /// <response code="401">You have not logged in.</response>
        /// <response code="403">You are not administrator.</response>
        [HttpDelete("users/{username}/avatar")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute][Username] string username)
        {
            if (!this.UserHasPermission(UserPermission.UserManagement) && User.Identity!.Name != username)
            {
                return this.ForbidWithMessage(Resource.MessageForbidNotAdministratorOrOwner);
            }

            long id = await _userService.GetUserIdByUsernameAsync(username);

            await _service.DeleteAvatarAsync(id);
            return Ok();
        }
    }
}
