using System;
using System.Threading.Tasks;

namespace Timeline.Services.Token
{
    public interface IUserTokenService
    {
        /// <summary>
        /// Create a token for a user. Please ensure the user id exists!
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="expireTime">The expire time of the token.</param>
        /// <returns>Return the generated token.</returns>
        Task<string> CreateTokenAsync(long userId, DateTime? expireTime);

        /// <summary>
        /// Verify a token and get the info of the token.
        /// </summary>
        /// <param name="token">The token to verify.</param>
        /// <returns>The info of the token.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="token"/> is null.</exception>
        /// <exception cref="UserTokenException">Thrown when the token is not valid for reasons other than expired.</exception>
        /// <exception cref="UserTokenExpiredException">Thrown when the token is expired.</exception>
        Task<UserTokenInfo> ValidateTokenAsync(string token);

        /// <summary>
        /// Revoke a token to make it no longer valid.
        /// </summary>
        /// <param name="token">The token to revoke.</param>
        /// <returns>Return true if a token is revoked.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="token"/> is null.</exception>
        /// <remarks>
        /// This method returns true if a real token is revoked and returns false if the token is not valid.
        /// If the token is expired, false is return.
        /// </remarks>
        Task<bool> RevokeTokenAsync(string token);

        /// <summary>
        /// Revoke all tokens of a user.
        /// </summary>
        /// <param name="userId">User id of tokens.</param>
        /// <returns>Return the task.</returns>
        Task RevokeAllTokenByUserIdAsync(long userId);
    }
}
