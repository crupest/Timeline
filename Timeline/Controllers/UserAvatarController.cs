using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Authentication;
using Timeline.Filters;
using Timeline.Helpers;
using Timeline.Models.Http;
using Timeline.Models.Validation;
using Timeline.Services;

namespace Timeline
{
    public static partial class ErrorCodes
    {
        public static partial class Http
        {
            public static class UserAvatar // bbb = 003
            {
                public static class Get // cc = 01
                {
                    public const int UserNotExist = 10030101;
                }

                public static class Put // cc = 02
                {
                    public const int UserNotExist = 10030201;
                    public const int Forbid = 10030202;
                    public const int BadFormat_CantDecode = 10030203;
                    public const int BadFormat_UnmatchedFormat = 10030204;
                    public const int BadFormat_BadSize = 10030205;

                }

                public static class Delete // cc = 03
                {
                    public const int UserNotExist = 10030301;
                    public const int Forbid = 10030302;
                }
            }
        }
    }
}

namespace Timeline.Controllers
{
    [ApiController]
    public class UserAvatarController : Controller
    {
        private readonly ILogger<UserAvatarController> _logger;

        private readonly IUserAvatarService _service;

        private readonly IStringLocalizerFactory _localizerFactory;
        private readonly IStringLocalizer<UserAvatarController> _localizer;

        public UserAvatarController(ILogger<UserAvatarController> logger, IUserAvatarService service, IStringLocalizerFactory localizerFactory)
        {
            _logger = logger;
            _service = service;
            _localizerFactory = localizerFactory;
            _localizer = new StringLocalizer<UserAvatarController>(localizerFactory);
        }

        [HttpGet("users/{username}/avatar")]
        [ResponseCache(NoStore = false, Location = ResponseCacheLocation.None, Duration = 0)]
        public async Task<IActionResult> Get([FromRoute][Username] string username)
        {
            const string IfNonMatchHeaderKey = "If-None-Match";

            try
            {
                var eTagValue = $"\"{await _service.GetAvatarETag(username)}\"";
                var eTag = new EntityTagHeaderValue(eTagValue);

                if (Request.Headers.TryGetValue(IfNonMatchHeaderKey, out var value))
                {
                    if (!EntityTagHeaderValue.TryParseStrictList(value, out var eTagList))
                    {
                        _logger.LogInformation(Log.Format(Resources.Controllers.UserAvatarController.LogGetBadIfNoneMatch,
                            ("Username", username), ("If-None-Match", value)));
                        return BadRequest(HeaderErrorResponse.BadIfNonMatch(_localizerFactory));
                    }

                    if (eTagList.FirstOrDefault(e => e.Equals(eTag)) != null)
                    {
                        Response.Headers.Add("ETag", eTagValue);
                        _logger.LogInformation(Log.Format(Resources.Controllers.UserAvatarController.LogGetReturnNotModify, ("Username", username)));
                        return StatusCode(StatusCodes.Status304NotModified);
                    }
                }

                var avatarInfo = await _service.GetAvatar(username);
                var avatar = avatarInfo.Avatar;

                _logger.LogInformation(Log.Format(Resources.Controllers.UserAvatarController.LogGetReturnData, ("Username", username)));
                return File(avatar.Data, avatar.Type, new DateTimeOffset(avatarInfo.LastModified), eTag);
            }
            catch (UserNotExistException e)
            {
                _logger.LogInformation(e, Log.Format(Resources.Controllers.UserAvatarController.LogGetUserNotExist, ("Username", username)));
                return NotFound(new CommonResponse(ErrorCodes.Http.UserAvatar.Get.UserNotExist, _localizer["ErrorGetUserNotExist"]));
            }
        }

        [HttpPut("users/{username}/avatar")]
        [Authorize]
        [RequireContentType, RequireContentLength]
        [Consumes("image/png", "image/jpeg", "image/gif", "image/webp")]
        public async Task<IActionResult> Put([FromRoute][Username] string username)
        {
            var contentLength = Request.ContentLength!.Value;
            if (contentLength > 1000 * 1000 * 10)
                return BadRequest(ContentErrorResponse.TooBig(_localizerFactory, "10MB"));

            if (!User.IsAdministrator() && User.Identity.Name != username)
            {
                _logger.LogInformation(Log.Format(Resources.Controllers.UserAvatarController.LogPutForbid,
                    ("Operator Username", User.Identity.Name), ("Username To Put Avatar", username)));
                return StatusCode(StatusCodes.Status403Forbidden,
                    new CommonResponse(ErrorCodes.Http.UserAvatar.Put.Forbid, _localizer["ErrorPutForbid"]));
            }

            try
            {
                var data = new byte[contentLength];
                var bytesRead = await Request.Body.ReadAsync(data);

                if (bytesRead != contentLength)
                    return BadRequest(ContentErrorResponse.UnmatchedLength_Smaller(_localizerFactory));

                var extraByte = new byte[1];
                if (await Request.Body.ReadAsync(extraByte) != 0)
                    return BadRequest(ContentErrorResponse.UnmatchedLength_Bigger(_localizerFactory));

                await _service.SetAvatar(username, new Avatar
                {
                    Data = data,
                    Type = Request.ContentType
                });

                _logger.LogInformation(Log.Format(Resources.Controllers.UserAvatarController.LogPutSuccess,
                    ("Username", username), ("Mime Type", Request.ContentType)));
                return Ok();
            }
            catch (UserNotExistException e)
            {
                _logger.LogInformation(e, Log.Format(Resources.Controllers.UserAvatarController.LogPutUserNotExist, ("Username", username)));
                return BadRequest(new CommonResponse(ErrorCodes.Http.UserAvatar.Put.UserNotExist, _localizer["ErrorPutUserNotExist"]));
            }
            catch (AvatarFormatException e)
            {
                var (code, message) = e.Error switch
                {
                    AvatarFormatException.ErrorReason.CantDecode =>
                        (ErrorCodes.Http.UserAvatar.Put.BadFormat_CantDecode, _localizer["ErrorPutBadFormatCantDecode"]),
                    AvatarFormatException.ErrorReason.UnmatchedFormat =>
                       (ErrorCodes.Http.UserAvatar.Put.BadFormat_UnmatchedFormat, _localizer["ErrorPutBadFormatUnmatchedFormat"]),
                    AvatarFormatException.ErrorReason.BadSize =>
                       (ErrorCodes.Http.UserAvatar.Put.BadFormat_BadSize, _localizer["ErrorPutBadFormatBadSize"]),
                    _ =>
                        throw new Exception(Resources.Controllers.UserAvatarController.ExceptionUnknownAvatarFormatError)
                };

                _logger.LogInformation(e, Log.Format(Resources.Controllers.UserAvatarController.LogPutUserBadFormat, ("Username", username)));
                return BadRequest(new CommonResponse(code, message));
            }
        }

        [HttpDelete("users/{username}/avatar")]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute][Username] string username)
        {
            if (!User.IsAdministrator() && User.Identity.Name != username)
            {
                _logger.LogInformation(Log.Format(Resources.Controllers.UserAvatarController.LogPutUserBadFormat,
                    ("Operator Username", User.Identity.Name), ("Username To Delete Avatar", username)));
                return StatusCode(StatusCodes.Status403Forbidden,
                    new CommonResponse(ErrorCodes.Http.UserAvatar.Delete.Forbid, _localizer["ErrorDeleteForbid"]));
            }

            try
            {
                await _service.SetAvatar(username, null);

                _logger.LogInformation(Log.Format(Resources.Controllers.UserAvatarController.LogDeleteSuccess, ("Username", username)));
                return Ok();
            }
            catch (UserNotExistException e)
            {
                _logger.LogInformation(e, Log.Format(Resources.Controllers.UserAvatarController.LogDeleteNotExist, ("Username", username)));
                return BadRequest(new CommonResponse(ErrorCodes.Http.UserAvatar.Delete.UserNotExist, _localizer["ErrorDeleteUserNotExist"]));
            }
        }
    }
}
