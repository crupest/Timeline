using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Timeline.Configs;
using Timeline.Entities;
using Timeline.Helpers;
using Timeline.Services.User;

namespace Timeline.Services.Token
{
    public class UserTokenManager : IUserTokenManager
    {
        private readonly ILogger<UserTokenManager> _logger;
        private readonly IOptionsMonitor<TokenOptions> _tokenOptionsMonitor;
        private readonly IUserService _userService;
        private readonly IUserTokenHandler _userTokenService;
        private readonly IClock _clock;

        public UserTokenManager(ILogger<UserTokenManager> logger, IOptionsMonitor<TokenOptions> tokenOptionsMonitor, IUserService userService, IUserTokenHandler userTokenService, IClock clock)
        {
            _logger = logger;
            _tokenOptionsMonitor = tokenOptionsMonitor;
            _userService = userService;
            _userTokenService = userTokenService;
            _clock = clock;
        }

        public async Task<UserTokenCreateResult> CreateTokenAsync(string username, string password, DateTime? expireAt = null)
        {
            expireAt = expireAt?.MyToUtc();

            if (username == null)
                throw new ArgumentNullException(nameof(username));
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            var userId = await _userService.VerifyCredential(username, password);
            var user = await _userService.GetUserAsync(userId);

            var token = _userTokenService.GenerateToken(new UserTokenInfo
            {
                Id = user.Id,
                Version = user.Version,
                ExpireAt = expireAt ?? _clock.GetCurrentTime() + TimeSpan.FromSeconds(_tokenOptionsMonitor.CurrentValue.DefaultExpireSeconds)
            });

            _logger.LogInformation(Resource.LogTokenCreate, user.Username, userId);

            return new UserTokenCreateResult { Token = token, User = user };
        }


        public async Task<UserEntity> VerifyTokenAsync(string token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            UserTokenInfo tokenInfo;

            try
            {
                tokenInfo = _userTokenService.VerifyToken(token);
            }
            catch (UserTokenBadFormatException e)
            {
                _logger.LogInformation(e, Resource.LogTokenVerifiedFail);
                throw;
            }

            var currentTime = _clock.GetCurrentTime();
            if (tokenInfo.ExpireAt < currentTime)
            {
                var e = new UserTokenTimeExpiredException(token, tokenInfo.ExpireAt, currentTime);
                _logger.LogInformation(e, Resource.LogTokenVerifiedFail);
                throw e;
            }

            try
            {
                var user = await _userService.GetUserAsync(tokenInfo.Id);

                if (tokenInfo.Version < user.Version)
                {
                    var e = new UserTokenVersionExpiredException(token, tokenInfo.Version, user.Version);
                    _logger.LogInformation(e, Resource.LogTokenVerifiedFail);
                    throw e;
                }

                _logger.LogInformation(Resource.LogTokenVerified, user.Username, user.Id);

                return user;
            }
            catch (EntityNotExistException e)
            {
                var exception = new UserTokenUserNotExistException(token, e);
                _logger.LogInformation(exception, Resource.LogTokenVerifiedFail);
                throw exception;
            }
        }
    }
}
