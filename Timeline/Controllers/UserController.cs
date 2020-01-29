using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Timeline.Auth;
using Timeline.Helpers;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Models.Validation;
using Timeline.Services;
using static Timeline.Resources.Controllers.UserController;

namespace Timeline.Controllers
{
    [ApiController]
    public class UserController : Controller
    {

        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;

        public UserController(ILogger<UserController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [HttpGet("users"), AdminAuthorize]
        public async Task<ActionResult<User[]>> List()
        {
            return Ok(await _userService.GetUsers());
        }

        [HttpGet("users/{username}"), AdminAuthorize]
        public async Task<ActionResult<User>> Get([FromRoute][Username] string username)
        {
            try
            {
                var user = await _userService.GetUserByUsername(username);
                return Ok(user);
            }
            catch (UserNotExistException e)
            {
                _logger.LogInformation(e, Log.Format(LogGetUserNotExist, ("Username", username)));
                return NotFound(ErrorResponse.UserCommon.NotExist());
            }
        }

        [HttpPut("users/{username}"), AdminAuthorize]
        public async Task<ActionResult<CommonPutResponse>> Put([FromBody] UserPutRequest request, [FromRoute][Username] string username)
        {
            var result = await _userService.PutUser(username, request.Password, request.Administrator!.Value);
            switch (result)
            {
                case PutResult.Create:
                    return CreatedAtAction("Get", new { username }, CommonPutResponse.Create());
                case PutResult.Modify:
                    return Ok(CommonPutResponse.Modify());
                default:
                    throw new Exception(ExceptionUnknownPutResult);
            }
        }

        [HttpPatch("users/{username}"), AdminAuthorize]
        public async Task<ActionResult> Patch([FromBody] UserPatchRequest request, [FromRoute][Username] string username)
        {
            try
            {
                await _userService.PatchUser(username, request.Password, request.Administrator);
                return Ok();
            }
            catch (UserNotExistException e)
            {
                _logger.LogInformation(e, Log.Format(LogPatchUserNotExist, ("Username", username)));
                return NotFound(ErrorResponse.UserCommon.NotExist());
            }
        }

        [HttpDelete("users/{username}"), AdminAuthorize]
        public async Task<ActionResult<CommonDeleteResponse>> Delete([FromRoute][Username] string username)
        {
            try
            {
                await _userService.DeleteUser(username);
                return Ok(CommonDeleteResponse.Delete());
            }
            catch (UserNotExistException)
            {
                return Ok(CommonDeleteResponse.NotExist());
            }
        }

        [HttpPost("userop/changeusername"), AdminAuthorize]
        public async Task<ActionResult> ChangeUsername([FromBody] ChangeUsernameRequest request)
        {
            try
            {
                await _userService.ChangeUsername(request.OldUsername, request.NewUsername);
                return Ok();
            }
            catch (UserNotExistException e)
            {
                _logger.LogInformation(e, Log.Format(LogChangeUsernameNotExist,
                    ("Old Username", request.OldUsername), ("New Username", request.NewUsername)));
                return BadRequest(ErrorResponse.UserCommon.NotExist());
            }
            catch (ConfictException e)
            {
                _logger.LogInformation(e, Log.Format(LogChangeUsernameConflict,
                    ("Old Username", request.OldUsername), ("New Username", request.NewUsername)));
                return BadRequest(ErrorResponse.UserController.ChangeUsername_Conflict());
            }
            // there is no need to catch bad format exception because it is already checked in model validation.
        }

        [HttpPost("userop/changepassword"), Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                await _userService.ChangePassword(User.Identity.Name!, request.OldPassword, request.NewPassword);
                return Ok();
            }
            catch (BadPasswordException e)
            {
                _logger.LogInformation(e, Log.Format(LogChangePasswordBadPassword,
                    ("Username", User.Identity.Name), ("Old Password", request.OldPassword)));
                return BadRequest(ErrorResponse.UserController.ChangePassword_BadOldPassword());
            }
            // User can't be non-existent or the token is bad. 
        }
    }
}
