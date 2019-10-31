using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Timeline.Filters;
using Timeline.Models.Validation;
using Timeline.Services;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace Timeline.Controllers
{
    [ApiController]
    public class UserDetailController : Controller
    {
        private readonly IUserDetailService _service;

        public UserDetailController(IUserDetailService service)
        {
            _service = service;
        }

        [HttpGet("users/{username}/nickname")]
        [CatchUserNotExistException]
        public async Task<ActionResult<string>> GetNickname([FromRoute][Username] string username)
        {
            return Ok(await _service.GetNickname(username));
        }

        [HttpPut("users/{username}/nickname")]
        [Authorize]
        [SelfOrAdmin]
        [CatchUserNotExistException]
        public async Task<ActionResult> PutNickname([FromRoute][Username] string username,
            [FromBody][StringLength(10, MinimumLength = 1)] string body)
        {
            await _service.SetNickname(username, body);
            return Ok();
        }

        [HttpDelete("users/{username}/nickname")]
        [Authorize]
        [SelfOrAdmin]
        [CatchUserNotExistException]
        public async Task<ActionResult> DeleteNickname([FromRoute][Username] string username)
        {
            await _service.SetNickname(username, null);
            return Ok();
        }
    }
}
