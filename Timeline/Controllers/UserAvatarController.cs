using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Timeline.Auth;
using Timeline.Filters;
using Timeline.Helpers;
using Timeline.Models.Http;
using Timeline.Models.Validation;
using Timeline.Services;
using Timeline.Services.Exceptions;
using static Timeline.Resources.Controllers.UserAvatarController;

namespace Timeline.Controllers
{
    /// <summary>
    /// Operations about user avatar.
    /// </summary>
    [ApiController]
    [ProducesErrorResponseType(typeof(CommonResponse))]
    public class UserAvatarController : Controller
    {
        private readonly ILogger<UserAvatarController> _logger;

        private readonly IUserService _userService;
        private readonly IUserAvatarService _service;

        /// <summary>
        /// 
        /// </summary>
        public UserAvatarController(ILogger<UserAvatarController> logger, IUserService userService, IUserAvatarService service)
        {
            _logger = logger;
            _userService = userService;
            _service = service;
        }

        /// <summary>
        /// Get avatar of a user.
        /// </summary>
        /// <param name="username">Username of the user to get avatar of.</param>
        /// <param name="ifNoneMatch">If-None-Match header.</param>
        /// <response code="200">Succeeded to get the avatar.</response>
        /// <response code="304">The avatar does not change.</response>
        /// <response code="404">The user does not exist.</response>
        [HttpGet("users/{username}/avatar")]
        [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([FromRoute][Username] string username, [FromHeader(Name = "If-None-Match")] string? ifNoneMatch)
        {
            _ = ifNoneMatch;
            long id;
            try
            {
                id = await _userService.GetUserIdByUsername(username);
            }
            catch (UserNotExistException e)
            {
                _logger.LogInformation(e, Log.Format(LogGetUserNotExist, ("Username", username)));
                return NotFound(ErrorResponse.UserCommon.NotExist());
            }

            return await DataCacheHelper.GenerateActionResult(this, () => _service.GetAvatarETag(id), async () =>
            {
                var avatar = await _service.GetAvatar(id);
                return avatar.ToCacheableData();
            });
        }

        /// <summary>
        /// Set avatar of a user. You have to be administrator to change other's.
        /// </summary>
        /// <param name="username">Username of the user to set avatar of.</param>
        /// <response code="200">Succeeded to set avatar.</response>
        /// <response code="400">Error code is 10010001 if user does not exist. Or avatar is of bad format.</response>
        /// <response code="401">You have not logged in.</response>
        /// <response code="403">You are not administrator.</response>
        [HttpPut("users/{username}/avatar")]
        [Authorize]
        [RequireContentType, RequireContentLength]
        [Consumes("image/png", "image/jpeg", "image/gif", "image/webp")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Put([FromRoute][Username] string username)
        {
            var contentLength = Request.ContentLength!.Value;
            if (contentLength > 1000 * 1000 * 10)
                return BadRequest(ErrorResponse.Common.Content.TooBig("10MB"));

            if (!User.IsAdministrator() && User.Identity.Name != username)
            {
                _logger.LogInformation(Log.Format(LogPutForbid,
                    ("Operator Username", User.Identity.Name), ("Username To Put Avatar", username)));
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            long id;
            try
            {
                id = await _userService.GetUserIdByUsername(username);
            }
            catch (UserNotExistException e)
            {
                _logger.LogInformation(e, Log.Format(LogPutUserNotExist, ("Username", username)));
                return BadRequest(ErrorResponse.UserCommon.NotExist());
            }

            try
            {
                var data = new byte[contentLength];
                var bytesRead = await Request.Body.ReadAsync(data);

                if (bytesRead != contentLength)
                    return BadRequest(ErrorResponse.Common.Content.UnmatchedLength_Smaller());

                var extraByte = new byte[1];
                if (await Request.Body.ReadAsync(extraByte) != 0)
                    return BadRequest(ErrorResponse.Common.Content.UnmatchedLength_Bigger());

                await _service.SetAvatar(id, new Avatar
                {
                    Data = data,
                    Type = Request.ContentType
                });

                _logger.LogInformation(Log.Format(LogPutSuccess,
                    ("Username", username), ("Mime Type", Request.ContentType)));
                return Ok();
            }
            catch (ImageException e)
            {
                _logger.LogInformation(e, Log.Format(LogPutUserBadFormat, ("Username", username)));
                return BadRequest(e.Error switch
                {
                    ImageException.ErrorReason.CantDecode => ErrorResponse.UserAvatar.BadFormat_CantDecode(),
                    ImageException.ErrorReason.UnmatchedFormat => ErrorResponse.UserAvatar.BadFormat_UnmatchedFormat(),
                    ImageException.ErrorReason.NotSquare => ErrorResponse.UserAvatar.BadFormat_BadSize(),
                    _ =>
                        throw new Exception(ExceptionUnknownAvatarFormatError)
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
            if (!User.IsAdministrator() && User.Identity.Name != username)
            {
                _logger.LogInformation(Log.Format(LogDeleteForbid,
                    ("Operator Username", User.Identity.Name), ("Username To Delete Avatar", username)));
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.Common.Forbid());
            }

            long id;
            try
            {
                id = await _userService.GetUserIdByUsername(username);
            }
            catch (UserNotExistException e)
            {
                _logger.LogInformation(e, Log.Format(LogDeleteNotExist, ("Username", username)));
                return BadRequest(ErrorResponse.UserCommon.NotExist());
            }

            await _service.SetAvatar(id, null);
            return Ok();
        }
    }
}
