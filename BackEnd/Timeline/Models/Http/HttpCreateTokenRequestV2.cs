using System;
using System.ComponentModel.DataAnnotations;

namespace Timeline.Models.Http
{
    public class HttpCreateTokenRequestV2
    {
        /// <summary>
        /// The username.
        /// </summary>
        [Required]
        public string Username { get; set; } = default!;
        /// <summary>
        /// The password.
        /// </summary>
        [Required]
        public string Password { get; set; } = default!;
        /// <summary>
        /// Optional token validation period. In days. If not specified, the token will be valid until being revoked explicited.
        /// </summary>
        [Range(1, 365)]
        public int? ValidDays { get; set; }
    }
}

