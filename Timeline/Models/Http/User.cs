using System.ComponentModel.DataAnnotations;
using Timeline.Models.Validation;

namespace Timeline.Models.Http
{
    public class UserPutRequest
    {
        [Required]
        public string Password { get; set; }
        [Required]
        public bool? Administrator { get; set; }
    }

    public class UserPatchRequest
    {
        public string Password { get; set; }
        public bool? Administrator { get; set; }
    }

    public class ChangeUsernameRequest
    {
        [Required]
        public string OldUsername { get; set; }

        [Required, ValidateWith(typeof(UsernameValidator))]
        public string NewUsername { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required]
        public string OldPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
