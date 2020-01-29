using System.ComponentModel.DataAnnotations;
using Timeline.Models.Validation;

namespace Timeline.Models.Http
{
    public class UserPatchRequest
    {
        [Username]
        public string? Username { get; set; }

        [MinLength(1)]
        public string? Password { get; set; }

        [Nickname]
        public string? Nickname { get; set; }

        public bool? Administrator { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required(AllowEmptyStrings = false)]
        public string OldPassword { get; set; } = default!;
        [Required(AllowEmptyStrings = false)]
        public string NewPassword { get; set; } = default!;
    }
}
