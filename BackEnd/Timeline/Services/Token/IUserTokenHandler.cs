using System;

namespace Timeline.Services.Token
{
    public interface IUserTokenHandler
    {
        /// <summary>
        /// Create a token for a given token info.
        /// </summary>
        /// <param name="tokenInfo">The info to generate token.</param>
        /// <returns>Return the generated token.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tokenInfo"/> is null.</exception>
        string GenerateToken(UserTokenInfo tokenInfo);

        /// <summary>
        /// Verify a token and get the saved info. Do not validate lifetime!!!
        /// </summary>
        /// <param name="token">The token to verify.</param>
        /// <returns>The saved info in token.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="token"/> is null.</exception>
        /// <exception cref="UserTokenBadFormatException">Thrown when the token is of bad format.</exception>
        /// <remarks>
        /// If this method throw <see cref="UserTokenBadFormatException"/>, it usually means the token is not created by this service.
        /// Do not check expire time in this method, only check whether it is present.
        /// </remarks>
        UserTokenInfo VerifyToken(string token);
    }
}
