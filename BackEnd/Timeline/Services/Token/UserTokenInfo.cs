using System;

namespace Timeline.Services.Token
{
    public class UserTokenInfo
    {
        public long UserId { get; set; }
        public DateTime? ExpireAt { get; set; }
        public DateTime? CreateAt { get; set; }
    }
}
