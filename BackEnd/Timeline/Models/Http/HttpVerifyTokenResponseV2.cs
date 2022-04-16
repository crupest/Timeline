
using System;

namespace Timeline.Models.Http
{

    public class HttpVerifyTokenResponseV2
    {
        /// <summary>
        /// The user owning the token.
        /// </summary>
        public HttpUser User { get; set; } = default!;

        public DateTime? ExpireAt { get; set; }
    }
}
