using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Timeline.Configs;
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

        public class UserCredentials
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        private readonly IOptionsMonitor<JwtConfig> _jwtConfig;
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IOptionsMonitor<JwtConfig> jwtConfig, IUserService userService, ILogger<UserController> logger)
        {
            _jwtConfig = jwtConfig;
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("[action]")]
        [AllowAnonymous]
        public IActionResult LogIn([FromBody] UserCredentials credentials)
        {
            var user = _userService.Authenticate(credentials.Username, credentials.Password);

            if (user == null) {
                _logger.LogInformation(LoggingEventIds.LogInFailed, "Attemp to login with username: {} and password: {} failed.", credentials.Username, credentials.Password);
                return BadRequest();
            }

            _logger.LogInformation(LoggingEventIds.LogInSucceeded, "Login with username: {} succeeded.");

            var jwtConfig = _jwtConfig.CurrentValue;

            var handler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[]{ new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) }),
                Issuer = jwtConfig.Issuer,
                Audience = jwtConfig.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtConfig.SigningKey)), SecurityAlgorithms.HmacSha384),
                IssuedAt = DateTime.Now,
                Expires = DateTime.Now.AddDays(1)
            };

            var token = handler.CreateToken(tokenDescriptor);
            var tokenString = handler.WriteToken(token);

            Response.Headers.Append("Authorization", "Bearer " + tokenString);

            return Ok();
        }
    }
}
