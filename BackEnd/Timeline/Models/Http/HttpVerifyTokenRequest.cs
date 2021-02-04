using Timeline.Controllers;

namespace Timeline.Models.Http
{
    /// <summary>
    /// Request model for <see cref="TokenController.Verify(HttpVerifyTokenRequest)"/>.
    /// </summary>
    public class HttpVerifyTokenRequest
    {
        /// <summary>
        /// The token to verify.
        /// </summary>
        public string Token { get; set; } = default!;
    }
}
