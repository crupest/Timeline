using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Entities.Http;
using Timeline.Services;

namespace Timeline.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("users"), Authorize(Roles = "admin")]
        public async Task<ActionResult<UserInfo[]>> List()
        {
            return Ok(await _userService.ListUsers());
        }

        [HttpGet("user/{username}"), Authorize]
        public async Task<IActionResult> Get([FromRoute] string username)
        {
            var user = await _userService.GetUser(username);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPut("user/{username}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> Put([FromBody] UserPutRequest request, [FromRoute] string username)
        {
            var result = await _userService.PutUser(username, request.Password, request.IsAdmin);
            switch (result)
            {
                case PutUserResult.Created:
                    return CreatedAtAction("Get", new { username }, UserPutResponse.Created);
                case PutUserResult.Modified:
                    return Ok(UserPutResponse.Modified);
                default:
                    throw new Exception("Unreachable code.");
            }
        }

        [HttpPatch("user/{username}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> Patch([FromBody] UserPatchRequest request, [FromRoute] string username)
        {
            var result = await _userService.PatchUser(username, request.Password, request.IsAdmin);
            switch (result)
            {
                case PatchUserResult.Success:
                    return Ok();
                case PatchUserResult.NotExists:
                    return NotFound();
                default:
                    throw new Exception("Unreachable code.");
            }
        }

        [HttpDelete("user/{username}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete([FromRoute] string username)
        {
            var result = await _userService.DeleteUser(username);
            switch (result)
            {
                case DeleteUserResult.Deleted:
                    return Ok(UserDeleteResponse.Deleted);
                case DeleteUserResult.NotExists:
                    return Ok(UserDeleteResponse.NotExists);
                default:
                    throw new Exception("Uncreachable code.");
            }
        }

        [HttpGet("user/{username}/avatar"), Authorize]
        public async Task<IActionResult> GetAvatar([FromRoute] string username)
        {
            var url = await _userService.GetAvatarUrl(username);
            if (url == null)
                return NotFound();
            return Redirect(url);
        }

        [HttpPut("user/{username}/avatar"), Authorize]
        [Consumes("image/png", "image/gif", "image/jpeg", "image/svg+xml")]
        public async Task<IActionResult> PutAvatar([FromRoute] string username, [FromHeader(Name="Content-Type")] string contentType)
        {
            bool isAdmin = User.IsInRole("admin");
            if (!isAdmin)
            {
                if (username != User.Identity.Name)
                    return StatusCode(StatusCodes.Status403Forbidden, PutAvatarResponse.Forbidden);
            }

            var stream = new MemoryStream();
            await Request.Body.CopyToAsync(stream);
            var result = await _userService.PutAvatar(username, stream.ToArray(), contentType);
            switch (result)
            {
                case PutAvatarResult.Success:
                    return Ok(PutAvatarResponse.Success);
                case PutAvatarResult.UserNotExists:
                    return BadRequest(PutAvatarResponse.NotExists);
                default:
                    throw new Exception("Unknown put avatar result.");
            }
        }


        [HttpPost("userop/changepassword"), Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var result = await _userService.ChangePassword(User.Identity.Name, request.OldPassword, request.NewPassword);
            switch (result)
            {
                case ChangePasswordResult.Success:
                    return Ok(ChangePasswordResponse.Success);
                case ChangePasswordResult.BadOldPassword:
                    return Ok(ChangePasswordResponse.BadOldPassword);
                case ChangePasswordResult.NotExists:
                    return Ok(ChangePasswordResponse.NotExists);
                default:
                    throw new Exception("Uncreachable code.");
            }
        }
    }
}
