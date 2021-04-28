using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Timeline.Helpers;
using Timeline.Models.Http;
using Timeline.Services;
using Timeline.Services.Mapper;
using Timeline.Services.Token;
using Timeline.Services.User;
using static Timeline.Resources.Controllers.TokenController;

namespace Timeline.Controllers
{
    /// <summary>
    /// Operation about tokens.
    /// </summary>
    [Route("token")]
    [ApiController]
    [ProducesErrorResponseType(typeof(CommonResponse))]
    public class TokenController : Controller
    {
        private readonly IUserTokenManager _userTokenManager;
        private readonly ILogger<TokenController> _logger;
        private readonly UserMapper _userMapper;
        private readonly IClock _clock;

        /// <summary></summary>
        public TokenController(IUserTokenManager userTokenManager, ILogger<TokenController> logger, UserMapper userMapper, IClock clock)
        {
            _userTokenManager = userTokenManager;
            _logger = logger;
            _userMapper = userMapper;
            _clock = clock;
        }

        /// <summary>
        /// Create a new token for a user.
        /// </summary>
        /// <returns>Result of token creation.</returns>
        [HttpPost("create")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<HttpCreateTokenResponse>> Create([FromBody] HttpCreateTokenRequest request)
        {
            void LogFailure(string reason, Exception? e = null)
            {
                _logger.LogInformation(e, Log.Format(LogCreateFailure,
                    ("Reason", reason),
                    ("Username", request.Username),
                    ("Password", request.Password),
                    ("Expire (in days)", request.Expire)
                ));
            }

            try
            {
                DateTime? expireTime = null;
                if (request.Expire != null)
                    expireTime = _clock.GetCurrentTime().AddDays(request.Expire.Value);

                var result = await _userTokenManager.CreateTokenAsync(request.Username, request.Password, expireTime);

                _logger.LogInformation(Log.Format(LogCreateSuccess,
                    ("Username", request.Username),
                    ("Expire At", expireTime?.ToString(CultureInfo.CurrentCulture.DateTimeFormat) ?? "default")
                ));
                return new HttpCreateTokenResponse
                {
                    Token = result.Token,
                    User = await _userMapper.MapToHttp(result.User, Url)
                };
            }
            catch (UserNotExistException e)
            {
                LogFailure(LogUserNotExist, e);
                return BadRequest(ErrorResponse.TokenController.Create_BadCredential());
            }
            catch (BadPasswordException e)
            {
                LogFailure(LogBadPassword, e);
                return BadRequest(ErrorResponse.TokenController.Create_BadCredential());
            }
        }

        /// <summary>
        /// Verify a token.
        /// </summary>
        /// <returns>Result of token verification.</returns>
        [HttpPost("verify")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<HttpVerifyTokenResponse>> Verify([FromBody] HttpVerifyTokenRequest request)
        {
            void LogFailure(string reason, Exception? e = null, params (string, object?)[] otherProperties)
            {
                var properties = new (string, object?)[2 + otherProperties.Length];
                properties[0] = ("Reason", reason);
                properties[1] = ("Token", request.Token);
                otherProperties.CopyTo(properties, 2);
                _logger.LogInformation(e, Log.Format(LogVerifyFailure, properties));
            }

            try
            {
                var result = await _userTokenManager.VerifyTokenAsync(request.Token);
                _logger.LogInformation(Log.Format(LogVerifySuccess,
                    ("Username", result.Username), ("Token", request.Token)));
                return new HttpVerifyTokenResponse
                {
                    User = await _userMapper.MapToHttp(result, Url)
                };
            }
            catch (UserTokenTimeExpiredException e)
            {
                LogFailure(LogVerifyExpire, e, ("Expire Time", e.ExpireTime), ("Verify Time", e.VerifyTime));
                return BadRequest(ErrorResponse.TokenController.Verify_TimeExpired());
            }
            catch (UserTokenVersionExpiredException e)
            {
                LogFailure(LogVerifyOldVersion, e, ("Token Version", e.TokenVersion), ("Required Version", e.RequiredVersion));
                return BadRequest(ErrorResponse.TokenController.Verify_OldVersion());

            }
            catch (UserTokenBadFormatException e)
            {
                LogFailure(LogVerifyBadFormat, e);
                return BadRequest(ErrorResponse.TokenController.Verify_BadFormat());
            }
            catch (UserTokenUserNotExistException e)
            {
                LogFailure(LogVerifyUserNotExist, e);
                return BadRequest(ErrorResponse.TokenController.Verify_UserNotExist());
            }
        }
    }
}
