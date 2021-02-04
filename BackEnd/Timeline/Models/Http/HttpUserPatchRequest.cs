using System.ComponentModel.DataAnnotations;
using Timeline.Controllers;
using Timeline.Models.Validation;

namespace Timeline.Models.Http
{
    /// <summary>
    /// Request model for <see cref="UserController.Patch(HttpUserPatchRequest, string)"/>.
    /// </summary>
    public class HttpUserPatchRequest
    {
        /// <summary>
        /// New username. Null if not change. Need to be administrator.
        /// </summary>
        [Username]
        public string? Username { get; set; }

        /// <summary>
        /// New password. Null if not change. Need to be administrator.
        /// </summary>
        [MinLength(1)]
        public string? Password { get; set; }

        /// <summary>
        /// New nickname. Null if not change. Need to be administrator to change other's.
        /// </summary>
        [Nickname]
        public string? Nickname { get; set; }
    }
}
