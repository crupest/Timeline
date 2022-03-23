using System;

namespace Timeline.Services.Token
{
    [Serializable]
    public class UserTokenExpiredException : UserTokenException
    {
        public UserTokenExpiredException() : base(Resource.ExceptionUserTokenExpired) { }
        public UserTokenExpiredException(string message) : base(message) { }
        public UserTokenExpiredException(string message, Exception inner) : base(message, inner) { }
        public UserTokenExpiredException(string token, DateTime expireTime, DateTime verifyTime) : base(token, Resource.ExceptionUserTokenExpired) { ExpireTime = expireTime; VerifyTime = verifyTime; }
        public UserTokenExpiredException(string token, DateTime expireTime, DateTime verifyTime, Exception inner) : base(token, Resource.ExceptionUserTokenExpired, inner) { ExpireTime = expireTime; VerifyTime = verifyTime; }
        protected UserTokenExpiredException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public DateTime ExpireTime { get; private set; }

        public DateTime VerifyTime { get; private set; }
    }
}
