using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
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
        }

        private readonly ILogger<UserAvatarController> _logger;

        private readonly IUserAvatarService _service;

        public UserAvatarController(ILogger<UserAvatarController> logger, IUserAvatarService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpGet("users/{username}/avatar")]
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
        [Consumes("image/png", "image/jpeg", "image/gif", "image/webp")]
        public async Task<IActionResult> Put(string username)
        {
            try
            {
                var data = new byte[Convert.ToInt32(Request.ContentLength)];
                await Request.Body.ReadAsync(data, 0, data.Length);

                await _service.SetAvatar(username, new Avatar
                {
                    Data = data,
                    Type = Request.ContentType
                });

                _logger.LogInformation($"Succeed to put a avatar of a user. Username: {username} . Mime Type: {Request.ContentType} .");
                return Ok();
            }
            catch (UserNotExistException)
            {
                _logger.LogInformation($"Attempt to put a avatar of a non-existent user failed. Username: {username} .");
                return BadRequest(new CommonResponse(ErrorCodes.Put_UserNotExist, "User does not exist."));
            }
        }
    }
}
