using Timeline.Controllers;

namespace Timeline.Models.Http
{

    /// <summary>
    /// Response model for <see cref="TokenController.Verify(HttpVerifyTokenRequest)"/>.
    /// </summary>
    public class HttpVerifyTokenResponse
    {
        /// <summary>
        /// The user owning the token.
        /// </summary>
        public HttpUser User { get; set; } = default!;
    }
}
