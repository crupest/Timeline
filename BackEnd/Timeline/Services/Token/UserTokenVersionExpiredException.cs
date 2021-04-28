using System;

namespace Timeline.Services.Token
{
    [Serializable]
    public class UserTokenVersionExpiredException : UserTokenException
    {
        public UserTokenVersionExpiredException() : base(Resource.ExceptionUserTokenVersionExpired) { }
        public UserTokenVersionExpiredException(string message) : base(message) { }
        public UserTokenVersionExpiredException(string message, Exception inner) : base(message, inner) { }
        public UserTokenVersionExpiredException(string token, long tokenVersion, long requiredVersion) : base(token, Resource.ExceptionUserTokenVersionExpired) { TokenVersion = tokenVersion; RequiredVersion = requiredVersion; }
        public UserTokenVersionExpiredException(string token, long tokenVersion, long requiredVersion, Exception inner) : base(token, Resource.ExceptionUserTokenVersionExpired, inner) { TokenVersion = tokenVersion; RequiredVersion = requiredVersion; }
        protected UserTokenVersionExpiredException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public long TokenVersion { get; set; }

        public long RequiredVersion { get; set; }
    }
}
