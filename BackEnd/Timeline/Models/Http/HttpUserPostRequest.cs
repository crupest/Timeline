using System.ComponentModel.DataAnnotations;
using Timeline.Controllers;
using Timeline.Models.Validation;

namespace Timeline.Models.Http
{
    /// <summary>
    /// Request model for <see cref="UserController.Post(HttpUserPostRequest)"/>.
    /// </summary>
    public class HttpUserPostRequest
    {
        /// <summary>
        /// Username of the new user.
        /// </summary>
        [Required, Username]
        public string Username { get; set; } = default!;

        /// <summary>
        /// Password of the new user.
        /// </summary>
        [Required, MinLength(1)]
        public string Password { get; set; } = default!;
    }
}
