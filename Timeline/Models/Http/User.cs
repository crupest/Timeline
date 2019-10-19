using System.ComponentModel.DataAnnotations;
using Timeline.Models.Validation;

namespace Timeline.Models.Http
{
    public class UserPutRequest
    {
        [Required]
        public string Password { get; set; } = default!;
        [Required]
        public bool? Administrator { get; set; }
    }

    public class UserPatchRequest
    {
        public string? Password { get; set; }
        public bool? Administrator { get; set; }
    }

    public class ChangeUsernameRequest
    {
        [Required]
        public string OldUsername { get; set; } = default!;

        [Required, ValidateWith(typeof(UsernameValidator))]
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
