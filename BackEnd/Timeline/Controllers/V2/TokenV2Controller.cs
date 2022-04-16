using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Timeline.Models.Http;
using Timeline.Services;
using Timeline.Services.Token;
using Timeline.Services.User;

namespace Timeline.Controllers.V2
{
    [ApiController]
    [Route("v2/token")]
    public class TokenV2Controller : V2ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserTokenService _userTokenService;
        private readonly IClock _clock;

        public TokenV2Controller(IUserService userService, IUserTokenService userTokenService, IClock clock)
        {
            _userService = userService;
            _userTokenService = userTokenService;
            _clock = clock;
        }

        private const string BadCredentialMessage = "Username or password is wrong.";

        /// <summary>
        /// Create a new token for a user.
        /// </summary>
        /// <returns>Result of token creation.</returns>
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<HttpCreateTokenResponse>> CreateAsync([FromBody] HttpCreateTokenRequestV2 request)
        {

            try
            {
                DateTime? expireTime = null;
                if (request.ValidDays is not null)
                    expireTime = _clock.GetCurrentTime().AddDays(request.ValidDays.Value);

                var userId = await _userService.VerifyCredential(request.Username, request.Password);
                var token = await _userTokenService.CreateTokenAsync(userId, expireTime);
                var user = await _userService.GetUserAsync(userId);

                return new HttpCreateTokenResponse
                {
                    Token = token,
                    User = await MapAsync<HttpUser>(user)
                };
            }
            catch (EntityNotExistException)
            {
                return UnprocessableEntity(new ErrorResponse(ErrorResponse.InvalidRequest, BadCredentialMessage));
            }
            catch (BadPasswordException)
            {
                return UnprocessableEntity(new ErrorResponse(ErrorResponse.InvalidRequest, BadCredentialMessage));
            }
        }

        private const string TokenExpiredMessage = "The token has expired.";
        private const string TokenInvalidMessage = "The token is invalid.";

        /// <summary>
        /// Verify a token.
        /// </summary>
        /// <returns>Result of token verification.</returns>
        [HttpPost("verify")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<HttpVerifyTokenResponseV2>> VerifyAsync([FromBody] HttpVerifyOrRevokeTokenRequest request)
        {
            try
            {
                var tokenInfo = await _userTokenService.ValidateTokenAsync(request.Token);
                var user = await _userService.GetUserAsync(tokenInfo.UserId);
                return new HttpVerifyTokenResponseV2
                {
                    User = await MapAsync<HttpUser>(user),
                    ExpireAt = tokenInfo.ExpireAt
                };
            }
            catch (UserTokenExpiredException)
            {
                return UnprocessableEntity(new ErrorResponse(ErrorResponse.InvalidRequest, TokenExpiredMessage));
            }
            catch (UserTokenException)
            {
                return UnprocessableEntity(new ErrorResponse(ErrorResponse.InvalidRequest, TokenInvalidMessage));
            }
        }

        [HttpPost("revoke")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Authorize]
        public async Task<ActionResult> RevokeAsync([FromBody] HttpVerifyOrRevokeTokenRequest body)
        {
            UserTokenInfo userTokenInfo;
            try
            {
                userTokenInfo = await _userTokenService.ValidateTokenAsync(body.Token, false);
            }
            catch (UserTokenException)
            {
                return UnprocessableEntity(new ErrorResponse(ErrorResponse.InvalidRequest, TokenInvalidMessage));
            }

            if (userTokenInfo.UserId != GetAuthUserId())
                return UnprocessableEntity(new ErrorResponse(ErrorResponse.InvalidRequest, TokenInvalidMessage));

            await _userTokenService.RevokeTokenAsync(body.Token);

            return NoContent();
        }

        [HttpPost("revokeall")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize]
        public async Task<ActionResult> RevokeAllAsync()
        {
            await _userTokenService.RevokeAllTokenByUserIdAsync(GetAuthUserId());
            return NoContent();
        }
    }
}

