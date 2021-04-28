using System;

namespace Timeline.Services.Token
{
    [Serializable]
    public class UserTokenTimeExpiredException : UserTokenException
    {
        public UserTokenTimeExpiredException() : base(Resource.ExceptionUserTokenTimeExpired) { }
        public UserTokenTimeExpiredException(string message) : base(message) { }
        public UserTokenTimeExpiredException(string message, Exception inner) : base(message, inner) { }
        public UserTokenTimeExpiredException(string token, DateTime expireTime, DateTime verifyTime) : base(token, Resource.ExceptionUserTokenTimeExpired) { ExpireTime = expireTime; VerifyTime = verifyTime; }
        public UserTokenTimeExpiredException(string token, DateTime expireTime, DateTime verifyTime, Exception inner) : base(token, Resource.ExceptionUserTokenTimeExpired, inner) { ExpireTime = expireTime; VerifyTime = verifyTime; }
        protected UserTokenTimeExpiredException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public DateTime ExpireTime { get; private set; }

        public DateTime VerifyTime { get; private set; }
    }
}
