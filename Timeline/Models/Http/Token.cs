using System.ComponentModel.DataAnnotations;

namespace Timeline.Models.Http
{
    public class CreateTokenRequest
    {
        [Required]
        public string Username { get; set; } = default!;
        [Required]
        public string Password { get; set; } = default!;
        // in days, optional
        [Range(1, 365)]
        public int? Expire { get; set; }
    }

    public class CreateTokenResponse
    {
        public string Token { get; set; } = default!;
        public UserInfo User { get; set; } = default!;
    }

    public class VerifyTokenRequest
    {
        [Required]
        public string Token { get; set; } = default!;
    }

    public class VerifyTokenResponse
    {
        public UserInfo User { get; set; } = default!;
    }
}
