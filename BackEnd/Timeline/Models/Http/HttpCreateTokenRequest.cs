using System.ComponentModel.DataAnnotations;
using Timeline.Controllers;

namespace Timeline.Models.Http
{
    /// <summary>
    /// Request model for <see cref="TokenController.Create(HttpCreateTokenRequest)"/>.
    /// </summary>
    public class HttpCreateTokenRequest
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
}
