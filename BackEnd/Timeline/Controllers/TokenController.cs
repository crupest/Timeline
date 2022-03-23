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
    public class TokenController : MyControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserTokenService _userTokenService;
        private readonly IGenericMapper _mapper;
        private readonly IClock _clock;

        public TokenController(IUserService userService, IUserTokenService userTokenService, IGenericMapper mapper, IClock clock)
        {
            _userService = userService;
            _userTokenService = userTokenService;
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

                var userId = await _userService.VerifyCredential(request.Username, request.Password);
                var token = await _userTokenService.CreateTokenAsync(userId, expireTime);
                var user = await _userService.GetUserAsync(userId);

                return new HttpCreateTokenResponse
                {
                    Token = token,
                    User = await _mapper.MapAsync<HttpUser>(user, Url, User)
                };
            }
            catch (EntityNotExistException)
            {
                return BadRequestWithCommonResponse(ErrorCodes.TokenController.CreateBadCredential, Resource.MessageTokenCreateBadCredential);
            }
            catch (BadPasswordException)
            {
                return BadRequestWithCommonResponse(ErrorCodes.TokenController.CreateBadCredential, Resource.MessageTokenCreateBadCredential);
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
                var tokenInfo = await _userTokenService.ValidateTokenAsync(request.Token);
                var user = await _userService.GetUserAsync(tokenInfo.UserId);
                return new HttpVerifyTokenResponse
                {
                    User = await _mapper.MapAsync<HttpUser>(user, Url, User)
                };
            }
            catch (UserTokenExpiredException)
            {
                return BadRequestWithCommonResponse(ErrorCodes.TokenController.VerifyExpired, Resource.MessageTokenVerifyExpired);
            }
            catch (UserTokenException)
            {
                return BadRequestWithCommonResponse(ErrorCodes.TokenController.VerifyInvalid, Resource.MessageTokenVerifyInvalid);
            }
        }
    }
}
