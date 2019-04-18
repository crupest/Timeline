using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Services;

namespace Timeline.Controllers
{
    [Route("admin")]
    [Authorize(Roles = "admin")]
    public class AdminUserController : Controller
    {
        private readonly IUserService _userService;

        public AdminUserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("users")]
        public async Task<ActionResult<UserInfo[]>> List()
        {
            return Ok(await _userService.ListUsers());
        }

        [HttpGet("user/{username}")]
        public async Task<IActionResult> Get([FromRoute] string username)
        {
            var user = await _userService.GetUser(username);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPut("user/{username}")]
        public async Task<IActionResult> Put([FromBody] AdminUserEntityRequest request, [FromRoute] string username)
        {
            var result = await _userService.PutUser(username, request.Password, request.Roles);
            switch (result)
            {
                case PutUserResult.Created:
                    return CreatedAtAction("Get", new { username }, AdminUserPutResponse.Created);
                case PutUserResult.Modified:
                    return Ok(AdminUserPutResponse.Modified);
                default:
                    throw new Exception("Unreachable code.");
            }
        }

        [HttpPatch("user/{username}")]
        public async Task<IActionResult> Patch([FromBody] AdminUserEntityRequest request, [FromRoute] string username)
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

        [HttpDelete("user/{username}")]
        public async Task<ActionResult<AdminUserDeleteResponse>> Delete([FromRoute] string username)
        {
            var result = await _userService.DeleteUser(username);
            switch (result)
            {
                case DeleteUserResult.Success:
                    return Ok(AdminUserDeleteResponse.Success);
                case DeleteUserResult.NotExists:
                    return Ok(AdminUserDeleteResponse.NotExists);
                default:
                    throw new Exception("Uncreachable code.");
            }
        }
    }
}
