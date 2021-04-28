using System;

namespace Timeline.Services.Token
{
    public class UserTokenInfo
    {
        public long Id { get; set; }
        public long Version { get; set; }
        public DateTime ExpireAt { get; set; }
    }
}
