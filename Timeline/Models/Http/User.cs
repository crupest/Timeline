using System;
using System.ComponentModel.DataAnnotations;
using Timeline.Models.Validation;

namespace Timeline.Models.Http
{
    [Obsolete("Remove this.")]
    public class UserPutRequest
    {
        [Required]
        public string Password { get; set; } = default!;
        [Required]
        public bool? Administrator { get; set; }
    }

    [Obsolete("Remove this.")]
    public class UserPatchRequest
    {
        public string? Password { get; set; }
        public bool? Administrator { get; set; }
    }

    public class ChangeUsernameRequest
    {
        [Required]
        [Username]
        public string OldUsername { get; set; } = default!;

        [Required]
        [Username]
        public string NewUsername { get; set; } = default!;
    }

    public class ChangePasswordRequest
    {
        [Required]
        public string OldPassword { get; set; } = default!;
        [Required]
        public string NewPassword { get; set; } = default!;
    }
}
