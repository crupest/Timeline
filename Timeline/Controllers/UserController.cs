using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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
        public async Task<IActionResult> Put([FromBody] UserModifyRequest request, [FromRoute] string username)
        {
            var result = await _userService.PutUser(username, request.Password, request.Roles);
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
        public async Task<IActionResult> Patch([FromBody] UserModifyRequest request, [FromRoute] string username)
        {
            var result = await _userService.PatchUser(username, request.Password, request.Roles);
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
    }
}
