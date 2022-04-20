using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Timeline.Models.Http;
using Timeline.Models.Validation;
using Timeline.Services.User;
using Timeline.Services.User.RegisterCode;

namespace Timeline.Controllers.V2
{
    [ApiController]
    public class RegisterCodeController : V2ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRegisterCodeService _registerCodeService;

        public RegisterCodeController(IUserService userService, IRegisterCodeService registerCodeService)
        {
            _userService = userService;
            _registerCodeService = registerCodeService;
        }

        [HttpPost("v2/register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<HttpUser>> RegisterAsync([FromBody] HttpRegisterCodeRegisterRequest body)
        {
            try
            {
                var user = await _registerCodeService.RegisterUserWithCode(new CreateUserParams(body.Username, body.Password) { Nickname = body.Nickname }, body.RegisterCode);
                return await MapAsync<HttpUser>(user);
            }
            catch (InvalidRegisterCodeException)
            {
                return UnprocessableEntity(new ErrorResponse(ErrorResponse.InvalidRequest, "Invalid register code."));
            }
        }

        [HttpGet("v2/users/{username}/registercode")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<HttpRegisterCode>> GetRegisterCodeAsync([FromRoute][Username] string username)
        {
            var userId = await _userService.GetUserIdByUsernameAsync(username);
            if (!UserHasPermission(UserPermission.UserManagement) && userId != GetAuthUserId())
            {
                return Forbid();
            }
            var registerCode = await _registerCodeService.GetCurrentCodeAsync(userId);
            return new HttpRegisterCode
            {
                RegisterCode = registerCode
            };
        }

        [HttpPost("v2/users/{username}/renewregistercode")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<HttpRegisterCode>> RenewRegisterCodeAsync([FromRoute][Username] string username)
        {
            var userId = await _userService.GetUserIdByUsernameAsync(username);
            if (!UserHasPermission(UserPermission.UserManagement) && userId != GetAuthUserId())
            {
                return Forbid();
            }
            var registerCode = await _registerCodeService.CreateNewCodeAsync(userId);
            return new HttpRegisterCode
            {
                RegisterCode = registerCode
            };
        }
    }
}
