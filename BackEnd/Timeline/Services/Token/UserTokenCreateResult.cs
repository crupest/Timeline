using Timeline.Entities;

namespace Timeline.Services.Token
{
    public class UserTokenCreateResult
    {
        public string Token { get; set; } = default!;
        public UserEntity User { get; set; } = default!;
    }
}
