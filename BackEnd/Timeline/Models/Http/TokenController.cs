using System.ComponentModel.DataAnnotations;
using Timeline.Controllers;

namespace Timeline.Models.Http
{
    /// <summary>
    /// Request model for <see cref="TokenController.Create(CreateTokenRequest)"/>.
    /// </summary>
    public class CreateTokenRequest
    {
        /// <summary>
        /// The username.
        /// </summary>
        public string Username { get; set; } = default!;
        /// <summary>
        /// The password.
        /// </summary>
        public string Password { get; set; } = default!;
        /// <summary>
        /// Optional token validation period. In days. If not specified, server will use a default one.
        /// </summary>
        [Range(1, 365)]
        public int? Expire { get; set; }
    }

    /// <summary>
    /// Response model for <see cref="TokenController.Create(CreateTokenRequest)"/>.
    /// </summary>
    public class CreateTokenResponse
    {
        /// <summary>
        /// The token created.
        /// </summary>
        public string Token { get; set; } = default!;
        /// <summary>
        /// The user owning the token.
        /// </summary>
        public UserInfo User { get; set; } = default!;
    }

    /// <summary>
    /// Request model for <see cref="TokenController.Verify(VerifyTokenRequest)"/>.
    /// </summary>
    public class VerifyTokenRequest
    {
        /// <summary>
        /// The token to verify.
        /// </summary>
        public string Token { get; set; } = default!;
    }

    /// <summary>
    /// Response model for <see cref="TokenController.Verify(VerifyTokenRequest)"/>.
    /// </summary>
    public class VerifyTokenResponse
    {
        /// <summary>
        /// The user owning the token.
        /// </summary>
        public UserInfo User { get; set; } = default!;
    }
}
