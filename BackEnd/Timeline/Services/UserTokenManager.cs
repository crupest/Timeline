using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Timeline.Helpers;
using Timeline.Models;
using Timeline.Services.Exceptions;

namespace Timeline.Services
{
    public class UserTokenCreateResult
    {
        public string Token { get; set; } = default!;
        public User User { get; set; } = default!;
    }

    public interface IUserTokenManager
    {
        /// <summary>
        /// Try to create a token for given username and password.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="expireAt">The expire time of the token.</param>
        /// <returns>The created token and the user info.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="username"/> or <paramref name="password"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="username"/> is of bad format.</exception>
        /// <exception cref="UserNotExistException">Thrown when the user with <paramref name="username"/> does not exist.</exception>
        /// <exception cref="BadPasswordException">Thrown when <paramref name="password"/> is wrong.</exception>
        public Task<UserTokenCreateResult> CreateToken(string username, string password, DateTime? expireAt = null);

        /// <summary>
        /// Verify a token and get the saved user info. This also check the database for existence of the user.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>The user stored in token.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="token"/> is null.</exception>
        /// <exception cref="UserTokenTimeExpireException">Thrown when the token is expired.</exception>
        /// <exception cref="UserTokenBadVersionException">Thrown when the token is of bad version.</exception>
        /// <exception cref="UserTokenBadFormatException">Thrown when the token is of bad format.</exception>
        /// <exception cref="UserNotExistException">Thrown when the user specified by the token does not exist. Usually the user had been deleted after the token was issued.</exception>
        public Task<User> VerifyToken(string token);
    }

    public class UserTokenManager : IUserTokenManager
    {
        private readonly ILogger<UserTokenManager> _logger;
        private readonly IUserService _userService;
        private readonly IUserCredentialService _userCredentialService;
        private readonly IUserTokenService _userTokenService;
        private readonly IClock _clock;

        public UserTokenManager(ILogger<UserTokenManager> logger, IUserService userService, IUserCredentialService userCredentialService, IUserTokenService userTokenService, IClock clock)
        {
            _logger = logger;
            _userService = userService;
            _userCredentialService = userCredentialService;
            _userTokenService = userTokenService;
            _clock = clock;
        }

        public async Task<UserTokenCreateResult> CreateToken(string username, string password, DateTime? expireAt = null)
        {
            expireAt = expireAt?.MyToUtc();

            if (username == null)
                throw new ArgumentNullException(nameof(username));
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            var userId = await _userCredentialService.VerifyCredential(username, password);
            var user = await _userService.GetUser(userId);
            var token = _userTokenService.GenerateToken(new UserTokenInfo { Id = user.Id, Version = user.Version, ExpireAt = expireAt });

            return new UserTokenCreateResult { Token = token, User = user };
        }


        public async Task<User> VerifyToken(string token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            var tokenInfo = _userTokenService.VerifyToken(token);

            if (tokenInfo.ExpireAt.HasValue)
            {
                var currentTime = _clock.GetCurrentTime();
                if (tokenInfo.ExpireAt < currentTime)
                    throw new UserTokenTimeExpireException(token, tokenInfo.ExpireAt.Value, currentTime);
            }

            var user = await _userService.GetUser(tokenInfo.Id);

            if (tokenInfo.Version < user.Version)
                throw new UserTokenBadVersionException(token, tokenInfo.Version, user.Version);

            return user;
        }
    }
}
