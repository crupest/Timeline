using Timeline.Controllers;

namespace Timeline.Models.Http
{
    /// <summary>
    /// Response model for <see cref="TokenController.Create(HttpCreateTokenRequest)"/>.
    /// </summary>
    public class HttpCreateTokenResponse
    {
        /// <summary>
        /// The token created.
        /// </summary>
        public string Token { get; set; } = default!;
        /// <summary>
        /// The user owning the token.
        /// </summary>
        public HttpUser User { get; set; } = default!;
    }
}
