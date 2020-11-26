using AutoMapper;
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
using Timeline.Services.Exceptions;
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
        private readonly IUserCredentialService _userCredentialService;
        private readonly IUserTokenManager _userTokenManager;
        private readonly ILogger<TokenController> _logger;
        private readonly IClock _clock;

        private readonly IMapper _mapper;

        /// <summary></summary>
        public TokenController(IUserCredentialService userCredentialService, IUserTokenManager userTokenManager, ILogger<TokenController> logger, IClock clock, IMapper mapper)
        {
            _userCredentialService = userCredentialService;
            _userTokenManager = userTokenManager;
            _logger = logger;
            _clock = clock;
            _mapper = mapper;
        }

        /// <summary>
        /// Create a new token for a user.
        /// </summary>
        /// <returns>Result of token creation.</returns>
        [HttpPost("create")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CreateTokenResponse>> Create([FromBody] CreateTokenRequest request)
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

                var result = await _userTokenManager.CreateToken(request.Username, request.Password, expireTime);

                _logger.LogInformation(Log.Format(LogCreateSuccess,
                    ("Username", request.Username),
                    ("Expire At", expireTime?.ToString(CultureInfo.CurrentCulture.DateTimeFormat) ?? "default")
                ));
                return Ok(new CreateTokenResponse
                {
                    Token = result.Token,
                    User = _mapper.Map<UserInfo>(result.User)
                });
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
        public async Task<ActionResult<VerifyTokenResponse>> Verify([FromBody] VerifyTokenRequest request)
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
                var result = await _userTokenManager.VerifyToken(request.Token);
                _logger.LogInformation(Log.Format(LogVerifySuccess,
                    ("Username", result.Username), ("Token", request.Token)));
                return Ok(new VerifyTokenResponse
                {
                    User = _mapper.Map<UserInfo>(result)
                });
            }
            catch (UserTokenTimeExpireException e)
            {
                LogFailure(LogVerifyExpire, e, ("Expire Time", e.ExpireTime), ("Verify Time", e.VerifyTime));
                return BadRequest(ErrorResponse.TokenController.Verify_TimeExpired());
            }
            catch (UserTokenBadVersionException e)
            {
                LogFailure(LogVerifyOldVersion, e, ("Token Version", e.TokenVersion), ("Required Version", e.RequiredVersion));
                return BadRequest(ErrorResponse.TokenController.Verify_OldVersion());

            }
            catch (UserTokenBadFormatException e)
            {
                LogFailure(LogVerifyBadFormat, e);
                return BadRequest(ErrorResponse.TokenController.Verify_BadFormat());
            }
            catch (UserNotExistException e)
            {
                LogFailure(LogVerifyUserNotExist, e);
                return BadRequest(ErrorResponse.TokenController.Verify_UserNotExist());
            }
        }
    }
}
