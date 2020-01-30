using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Auth;
using Timeline.Filters;
using Timeline.Helpers;
using Timeline.Models.Http;
using Timeline.Models.Validation;
using Timeline.Services;
using static Timeline.Resources.Controllers.UserAvatarController;

namespace Timeline.Controllers
{
    [ApiController]
    public class UserAvatarController : Controller
    {
        private readonly ILogger<UserAvatarController> _logger;

        private readonly IUserService _userService;
        private readonly IUserAvatarService _service;

        public UserAvatarController(ILogger<UserAvatarController> logger, IUserService userService, IUserAvatarService service)
        {
            _logger = logger;
            _userService = userService;
            _service = service;
        }

        [HttpGet("users/{username}/avatar")]
        [ResponseCache(NoStore = false, Location = ResponseCacheLocation.None, Duration = 0)]
        public async Task<IActionResult> Get([FromRoute][Username] string username)
        {
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

            const string IfNonMatchHeaderKey = "If-None-Match";

            var eTagValue = $"\"{await _service.GetAvatarETag(id)}\"";
            var eTag = new EntityTagHeaderValue(eTagValue);

            if (Request.Headers.TryGetValue(IfNonMatchHeaderKey, out var value))
            {
                if (!EntityTagHeaderValue.TryParseStrictList(value, out var eTagList))
                {
                    _logger.LogInformation(Log.Format(LogGetBadIfNoneMatch,
                        ("Username", username), ("If-None-Match", value)));
                    return BadRequest(ErrorResponse.Common.Header.IfNonMatch_BadFormat());
                }

                if (eTagList.FirstOrDefault(e => e.Equals(eTag)) != null)
                {
                    Response.Headers.Add("ETag", eTagValue);
                    _logger.LogInformation(Log.Format(LogGetReturnNotModify, ("Username", username)));
                    return StatusCode(StatusCodes.Status304NotModified);
                }
            }

            var avatarInfo = await _service.GetAvatar(id);
            var avatar = avatarInfo.Avatar;

            _logger.LogInformation(Log.Format(LogGetReturnData, ("Username", username)));
            return File(avatar.Data, avatar.Type, new DateTimeOffset(avatarInfo.LastModified), eTag);

        }

        [HttpPut("users/{username}/avatar")]
        [Authorize]
        [RequireContentLength]
        [Consumes("image/png", "image/jpeg", "image/gif", "image/webp")]
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
            catch (AvatarFormatException e)
            {
                _logger.LogInformation(e, Log.Format(LogPutUserBadFormat, ("Username", username)));
                return BadRequest(e.Error switch
                {
                    AvatarFormatException.ErrorReason.CantDecode => ErrorResponse.UserAvatar.BadFormat_CantDecode(),
                    AvatarFormatException.ErrorReason.UnmatchedFormat => ErrorResponse.UserAvatar.BadFormat_UnmatchedFormat(),
                    AvatarFormatException.ErrorReason.BadSize => ErrorResponse.UserAvatar.BadFormat_BadSize(),
                    _ =>
                        throw new Exception(ExceptionUnknownAvatarFormatError)
                });
            }
        }

        [HttpDelete("users/{username}/avatar")]
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
