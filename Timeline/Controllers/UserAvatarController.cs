using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Timeline.Authenticate;
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

            public const int Delete_UserNotExist = -3001;
            public const int Delete_Forbid = -3002;
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
        public async Task<IActionResult> Get(string username)
        {
            try
            {
                var avatar = await _service.GetAvatar(username);
                return File(avatar.Data, avatar.Type);
            }
            catch (UserNotExistException)
            {
                _logger.LogInformation($"Attempt to get a avatar of a non-existent user failed. Username: {username} .");
                return NotFound(new CommonResponse(ErrorCodes.Get_UserNotExist, "User does not exist."));
            }
        }

        [HttpPut("users/{username}/avatar")]
        [Authorize]
        [Consumes("image/png", "image/jpeg", "image/gif", "image/webp")]
        public async Task<IActionResult> Put(string username)
        {
            if (!User.IsAdmin() && User.Identity.Name != username)
            {
                _logger.LogInformation($"Attempt to put a avatar of other user as a non-admin failed. Operator Username: {User.Identity.Name} ;  Username To Put Avatar: {username} .");
                return StatusCode(StatusCodes.Status403Forbidden,
                    new CommonResponse(ErrorCodes.Put_Forbid, "Normal user can't change other's avatar."));
            }

            try
            {
                var data = new byte[Convert.ToInt32(Request.ContentLength)];
                await Request.Body.ReadAsync(data, 0, data.Length);

                await _service.SetAvatar(username, new Avatar
                {
                    Data = data,
                    Type = Request.ContentType
                });

                _logger.LogInformation($"Succeed to put a avatar of a user. Username: {username} ; Mime Type: {Request.ContentType} .");
                return Ok();
            }
            catch (UserNotExistException)
            {
                _logger.LogInformation($"Attempt to put a avatar of a non-existent user failed. Username: {username} .");
                return BadRequest(new CommonResponse(ErrorCodes.Put_UserNotExist, "User does not exist."));
            }
        }

        [HttpDelete("users/{username}/avatar")]
        [Authorize]
        public async Task<IActionResult> Delete(string username)
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
            catch (UserNotExistException)
            {
                _logger.LogInformation($"Attempt to delete a avatar of a non-existent user failed. Username: {username} .");
                return BadRequest(new CommonResponse(ErrorCodes.Delete_UserNotExist, "User does not exist."));
            }
        }
    }
}
