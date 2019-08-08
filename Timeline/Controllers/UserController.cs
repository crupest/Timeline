using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Timeline.Authenticate;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Services;
using static Timeline.Helpers.MyLogHelper;

namespace Timeline.Controllers
{
    [ApiController]
    public class UserController : Controller
    {
        private static class ErrorCodes
        {
            public const int Get_NotExists = -1001;

            public const int Put_NoPassword = -2001;

            public const int Patch_NotExists = -3001;

            public const int ChangePassword_BadOldPassword = -4001;
        }

        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;

        public UserController(ILogger<UserController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [HttpGet("users"), AdminAuthorize]
        public async Task<ActionResult<UserInfo[]>> List()
        {
            return Ok(await _userService.ListUsers());
        }

        [HttpGet("user/{username}"), AdminAuthorize]
        public async Task<IActionResult> Get([FromRoute] string username)
        {
            var user = await _userService.GetUser(username);
            if (user == null)
            {
                _logger.LogInformation(FormatLogMessage("Attempt to get a non-existent user.", Pair("Username", username)));
                return NotFound(new CommonResponse(ErrorCodes.Get_NotExists, "The user does not exist."));
            }
            return Ok(user);
        }

        [HttpPut("user/{username}"), AdminAuthorize]
        public async Task<IActionResult> Put([FromBody] UserPutRequest request, [FromRoute] string username)
        {
            if (request.Password == null) // This place will be refactored.
            {
                _logger.LogInformation("Attempt to put a user without a password. Username: {} .", username);
                return BadRequest();
            }

            var result = await _userService.PutUser(username, request.Password, request.Administrator);
            switch (result)
            {
                case PutResult.Created:
                    _logger.LogInformation(FormatLogMessage("A user is created.", Pair("Username", username)));
                    return CreatedAtAction("Get", new { username }, CommonPutResponse.Created);
                case PutResult.Modified:
                    _logger.LogInformation(FormatLogMessage("A user is modified.", Pair("Username", username)));
                    return Ok(CommonPutResponse.Modified);
                default:
                    throw new Exception("Unreachable code.");
            }
        }

        [HttpPatch("user/{username}"), AdminAuthorize]
        public async Task<IActionResult> Patch([FromBody] UserPatchRequest request, [FromRoute] string username)
        {
            try
            {
                await _userService.PatchUser(username, request.Password, request.Administrator);
                return Ok();
            }
            catch (UserNotExistException e)
            {
                _logger.LogInformation(e, FormatLogMessage("Attempt to patch a non-existent user.", Pair("Username", username)));
                return BadRequest(new CommonResponse(ErrorCodes.Patch_NotExists, "The user does not exist."));
            }
        }

        [HttpDelete("user/{username}"), AdminAuthorize]
        public async Task<IActionResult> Delete([FromRoute] string username)
        {
            try
            {
                await _userService.DeleteUser(username);
                _logger.LogInformation(FormatLogMessage("A user is deleted.", Pair("Username", username)));
                return Ok(CommonDeleteResponse.Deleted);
            }
            catch (UserNotExistException e)
            {
                _logger.LogInformation(e, FormatLogMessage("Attempt to delete a non-existent user.", Pair("Username", username)));
                return Ok(CommonDeleteResponse.NotExists);
            }
        }

        [HttpPost("userop/changepassword"), Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                await _userService.ChangePassword(User.Identity.Name, request.OldPassword, request.NewPassword);
                _logger.LogInformation(FormatLogMessage("A user changed password.", Pair("Username", User.Identity.Name)));
                return Ok();
            }
            catch (BadPasswordException e)
            {
                _logger.LogInformation(e, FormatLogMessage("A user attempt to change password but old password is wrong.",
                    Pair("Username", User.Identity.Name), Pair("Old Password", request.OldPassword)));
                return BadRequest(new CommonResponse(ErrorCodes.ChangePassword_BadOldPassword, "Old password is wrong."));
            }
            // User can't be non-existent or the token is bad. 
        }
    }
}
