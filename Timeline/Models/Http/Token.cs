using System.ComponentModel.DataAnnotations;

namespace Timeline.Models.Http
{
    public class CreateTokenRequest
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        // in days, optional
        [Range(1, 365)]
        public int? ExpireOffset { get; set; }
    }

    public class CreateTokenResponse
    {
        public string Token { get; set; }
        public UserInfo User { get; set; }
    }

    public class VerifyTokenRequest
    {
        [Required]
        public string Token { get; set; }
    }

    public class VerifyTokenResponse
    {
        public UserInfo User { get; set; }
    }
}
