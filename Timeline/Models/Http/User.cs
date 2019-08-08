using System.ComponentModel.DataAnnotations;

namespace Timeline.Models.Http
{
    public class UserPutRequest
    {
        [Required]
        public string Password { get; set; }
        [Required]
        public bool Administrator { get; set; }
    }

    public class UserPatchRequest
    {
        public string Password { get; set; }
        public bool? Administrator { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required]
        public string OldPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
