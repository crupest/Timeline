using System;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Services.User;

namespace Timeline.Services.Token
{
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
        public Task<UserTokenCreateResult> CreateTokenAsync(string username, string password, DateTime? expireAt = null);

        /// <summary>
        /// Verify a token and get the saved user info. This also check the database for existence of the user.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>The user stored in token.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="token"/> is null.</exception>
        /// <exception cref="UserTokenTimeExpiredException">Thrown when the token is expired.</exception>
        /// <exception cref="UserTokenVersionExpiredException">Thrown when the token is of bad version.</exception>
        /// <exception cref="UserTokenBadFormatException">Thrown when the token is of bad format.</exception>
        /// <exception cref="UserTokenUserNotExistException">Thrown when the user specified by the token does not exist. Usually the user had been deleted after the token was issued.</exception>
        public Task<UserEntity> VerifyTokenAsync(string token);
    }
}
