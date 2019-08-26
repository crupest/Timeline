using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Authenticate;
using Timeline.Filters;
using Timeline.Models.Http;
using Timeline.Services;

namespace Timeline.Controllers
{
    [ApiController]
    public class UserAvatarController : Controller
    {
        public static class ErrorCodes
        {
            public const int Get_UserNotExist = -1001;

            public const int Put_UserNotExist = -2001;
            public const int Put_Forbid = -2002;
            public const int Put_BadFormat_CantDecode = -2011;
            public const int Put_BadFormat_UnmatchedFormat = -2012;
            public const int Put_BadFormat_BadSize = -2013;
            public const int Put_Content_TooBig = -2021;
            public const int Put_Content_UnmatchedLength_Less = -2022;
            public const int Put_Content_UnmatchedLength_Bigger = -2023;

            public const int Delete_UserNotExist = -3001;
            public const int Delete_Forbid = -3002;


            public static int From(AvatarDataException.ErrorReason error)
            {
                switch (error)
                {
                    case AvatarDataException.ErrorReason.CantDecode:
                        return Put_BadFormat_CantDecode;
                    case AvatarDataException.ErrorReason.UnmatchedFormat:
                        return Put_BadFormat_UnmatchedFormat;
                    case AvatarDataException.ErrorReason.BadSize:
                        return Put_BadFormat_BadSize;
                    default:
                        throw new Exception("Unknown AvatarDataException.ErrorReason value.");
                }
            }
        }

        private readonly ILogger<UserAvatarController> _logger;

        private readonly IUserAvatarService _service;

        public UserAvatarController(ILogger<UserAvatarController> logger, IUserAvatarService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpGet("users/{username}/avatar")]
        [Authorize]
        public async Task<IActionResult> Get([FromRoute] string username)
        {
            const string IfNonMatchHeaderKey = "If-None-Match";
            try
            {
                var eTag = new EntityTagHeaderValue($"\"{await _service.GetAvatarETag(username)}\"");

                if (Request.Headers.TryGetValue(IfNonMatchHeaderKey, out var value))
                {
                    if (!EntityTagHeaderValue.TryParseStrictList(value, out var eTagList))
                        return BadRequest(CommonResponse.BadIfNonMatch());

                    if (eTagList.First(e => e.Equals(eTag)) != null)
                        return StatusCode(StatusCodes.Status304NotModified);
                }

                var avatarInfo = await _service.GetAvatar(username);
                var avatar = avatarInfo.Avatar;

                Response.Headers.Add("Cache-Control", "no-cache, must-revalidate, max-age=1");
                return File(avatar.Data, avatar.Type, new DateTimeOffset(avatarInfo.LastModified), eTag);
            }
            catch (UserNotExistException e)
            {
                _logger.LogInformation(e, $"Attempt to get a avatar of a non-existent user failed. Username: {username} .");
                return NotFound(new CommonResponse(ErrorCodes.Get_UserNotExist, "User does not exist."));
            }
        }

        [HttpPut("users/{username}/avatar")]
        [Authorize]
        [RequireContentType, RequireContentLength]
        [Consumes("image/png", "image/jpeg", "image/gif", "image/webp")]
        public async Task<IActionResult> Put(string username)
        {
            var contentLength = Request.ContentLength.Value;
            if (contentLength > 1000 * 1000 * 10)
                return BadRequest(new CommonResponse(ErrorCodes.Put_Content_TooBig,
                    "Content can't be bigger than 10MB."));

            if (!User.IsAdmin() && User.Identity.Name != username)
            {
                _logger.LogInformation($"Attempt to put a avatar of other user as a non-admin failed. Operator Username: {User.Identity.Name} ;  Username To Put Avatar: {username} .");
                return StatusCode(StatusCodes.Status403Forbidden,
                    new CommonResponse(ErrorCodes.Put_Forbid, "Normal user can't change other's avatar."));
            }

            try
            {
                var data = new byte[contentLength];
                var bytesRead = await Request.Body.ReadAsync(data);

                if (bytesRead != contentLength)
                    return BadRequest(new CommonResponse(ErrorCodes.Put_Content_UnmatchedLength_Less,
                        $"Content length in header is {contentLength} but actual length is {bytesRead}."));

                if (Request.Body.ReadByte() != -1)
                    return BadRequest(new CommonResponse(ErrorCodes.Put_Content_UnmatchedLength_Bigger,
                        $"Content length in header is {contentLength} but actual length is bigger than that."));

                await _service.SetAvatar(username, new Avatar
                {
                    Data = data,
                    Type = Request.ContentType
                });

                _logger.LogInformation($"Succeed to put a avatar of a user. Username: {username} ; Mime Type: {Request.ContentType} .");
                return Ok();
            }
            catch (UserNotExistException e)
            {
                _logger.LogInformation(e, $"Attempt to put a avatar of a non-existent user failed. Username: {username} .");
                return BadRequest(new CommonResponse(ErrorCodes.Put_UserNotExist, "User does not exist."));
            }
            catch (AvatarDataException e)
            {
                _logger.LogInformation(e, $"Attempt to put a avatar of a bad format failed. Username: {username} .");
                return BadRequest(new CommonResponse(ErrorCodes.From(e.Error), "Bad format."));
            }
        }

        [HttpDelete("users/{username}/avatar")]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute] string username)
        {
            if (!User.IsAdmin() && User.Identity.Name != username)
            {
                _logger.LogInformation($"Attempt to delete a avatar of other user as a non-admin failed. Operator Username: {User.Identity.Name} ;  Username To Put Avatar: {username} .");
                return StatusCode(StatusCodes.Status403Forbidden,
                    new CommonResponse(ErrorCodes.Delete_Forbid, "Normal user can't delete other's avatar."));
            }

            try
            {
                await _service.SetAvatar(username, null);

                _logger.LogInformation($"Succeed to delete a avatar of a user. Username: {username} .");
                return Ok();
            }
            catch (UserNotExistException e)
            {
                _logger.LogInformation(e, $"Attempt to delete a avatar of a non-existent user failed. Username: {username} .");
                return BadRequest(new CommonResponse(ErrorCodes.Delete_UserNotExist, "User does not exist."));
            }
        }
    }
}
