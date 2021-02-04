using System.ComponentModel.DataAnnotations;
using Timeline.Controllers;

namespace Timeline.Models.Http
{
    /// <summary>
    /// Request model for <see cref="UserController.ChangePassword(HttpChangePasswordRequest)"/>.
    /// </summary>
    public class HttpChangePasswordRequest
    {
        /// <summary>
        /// Old password.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string OldPassword { get; set; } = default!;

        /// <summary>
        /// New password.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string NewPassword { get; set; } = default!;
    }
}
