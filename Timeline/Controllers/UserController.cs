using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Services;

namespace Timeline.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private static class LoggingEventIds
        {
            public const int LogInSucceeded = 4000;
            public const int LogInFailed = 4001;
        }

        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<ActionResult<CreateTokenResponse>> CreateToken([FromBody] CreateTokenRequest request)
        {
            var result = await _userService.CreateToken(request.Username, request.Password);

            if (result == null)
            {
                _logger.LogInformation(LoggingEventIds.LogInFailed, "Attemp to login with username: {} and password: {} failed.", request.Username, request.Password);
                return Ok(new CreateTokenResponse
                {
                    Success = false
                });
            }

            _logger.LogInformation(LoggingEventIds.LogInSucceeded, "Login with username: {} succeeded.", request.Username);

            return Ok(new CreateTokenResponse
            {
                Success = true,
                Token = result.Token,
                UserInfo = result.UserInfo
            });
        }

        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenValidationResponse>> ValidateToken([FromBody] TokenValidationRequest request)
        {
            var result = await _userService.VerifyToken(request.Token);

            if (result == null)
            {
                return Ok(new TokenValidationResponse
                {
                    IsValid = false,
                });
            }

            return Ok(new TokenValidationResponse
            {
                IsValid = true,
                UserInfo = result
            });
        }

        [HttpPost("[action]")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<CreateUserResponse>> CreateUser([FromBody] CreateUserRequest request)
        {
            var result = await _userService.CreateUser(request.Username, request.Password, request.Roles);
            switch (result)
            {
                case CreateUserResult.Success:
                    return Ok(new CreateUserResponse { ReturnCode = CreateUserResponse.SuccessCode });
                case CreateUserResult.AlreadyExists:
                    return Ok(new CreateUserResponse { ReturnCode = CreateUserResponse.AlreadyExistsCode });
                default:
                    throw new Exception("Unreachable code.");
            }
        }
    }
}
