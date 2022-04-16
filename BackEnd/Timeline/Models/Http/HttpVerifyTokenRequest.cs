using Timeline.Controllers;

namespace Timeline.Models.Http
{
    public class HttpVerifyOrRevokeTokenRequest
    {
        /// <summary>
        /// The token to verify.
        /// </summary>
        public string Token { get; set; } = default!;
    }
}
