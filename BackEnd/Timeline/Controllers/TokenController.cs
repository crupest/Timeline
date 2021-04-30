using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Timeline.Models.Http;
using Timeline.Services;
using Timeline.Services.Mapper;
using Timeline.Services.Token;
using Timeline.Services.User;

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
        private readonly IGenericMapper _mapper;
        private readonly IClock _clock;

        public TokenController(IUserTokenManager userTokenManager, IGenericMapper mapper, IClock clock)
        {
            _userTokenManager = userTokenManager;
            _mapper = mapper;
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

            try
            {
                DateTime? expireTime = null;
                if (request.Expire is not null)
                    expireTime = _clock.GetCurrentTime().AddDays(request.Expire.Value);

                var result = await _userTokenManager.CreateTokenAsync(request.Username, request.Password, expireTime);

                return new HttpCreateTokenResponse
                {
                    Token = result.Token,
                    User = await _mapper.MapAsync<HttpUser>(result.User, Url, User)
                };
            }
            catch (EntityNotExistException)
            {
                return BadRequest(ErrorResponse.TokenController.Create_BadCredential());
            }
            catch (BadPasswordException)
            {
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
            try
            {
                var result = await _userTokenManager.VerifyTokenAsync(request.Token);
                return new HttpVerifyTokenResponse
                {
                    User = await _mapper.MapAsync<HttpUser>(result, Url, User)
                };
            }
            catch (UserTokenTimeExpiredException)
            {
                return BadRequest(ErrorResponse.TokenController.Verify_TimeExpired());
            }
            catch (UserTokenVersionExpiredException)
            {
                return BadRequest(ErrorResponse.TokenController.Verify_OldVersion());
            }
            catch (UserTokenBadFormatException)
            {
                return BadRequest(ErrorResponse.TokenController.Verify_BadFormat());
            }
            catch (UserTokenUserNotExistException)
            {
                return BadRequest(ErrorResponse.TokenController.Verify_UserNotExist());
            }
        }
    }
}
