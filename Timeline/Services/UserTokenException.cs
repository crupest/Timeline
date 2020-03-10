using System;

namespace Timeline.Services
{

    [Serializable]
    public class UserTokenException : Exception
    {
        public UserTokenException() { }
        public UserTokenException(string message) : base(message) { }
        public UserTokenException(string message, Exception inner) : base(message, inner) { }
        public UserTokenException(string token, string message) : base(message) { Token = token; }
        public UserTokenException(string token, string message, Exception inner) : base(message, inner) { Token = token; }
        protected UserTokenException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public string Token { get; private set; } = "";
    }


    [Serializable]
    public class UserTokenTimeExpireException : UserTokenException
    {
        public UserTokenTimeExpireException() : base(Resources.Services.Exception.UserTokenTimeExpireException) { }
        public UserTokenTimeExpireException(string message) : base(message) { }
        public UserTokenTimeExpireException(string message, Exception inner) : base(message, inner) { }
        public UserTokenTimeExpireException(string token, DateTime expireTime, DateTime verifyTime) : base(token, Resources.Services.Exception.UserTokenTimeExpireException) { ExpireTime = expireTime; VerifyTime = verifyTime; }
        public UserTokenTimeExpireException(string token, DateTime expireTime, DateTime verifyTime, Exception inner) : base(token, Resources.Services.Exception.UserTokenTimeExpireException, inner) { ExpireTime = expireTime; VerifyTime = verifyTime; }
        protected UserTokenTimeExpireException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public DateTime ExpireTime { get; private set; } = default;

        public DateTime VerifyTime { get; private set; } = default;
    }

    [Serializable]
    public class UserTokenBadVersionException : UserTokenException
    {
        public UserTokenBadVersionException() : base(Resources.Services.Exception.UserTokenBadVersionException) { }
        public UserTokenBadVersionException(string message) : base(message) { }
        public UserTokenBadVersionException(string message, Exception inner) : base(message, inner) { }
        public UserTokenBadVersionException(string token, long tokenVersion, long requiredVersion) : base(token, Resources.Services.Exception.UserTokenBadVersionException) { TokenVersion = tokenVersion; RequiredVersion = requiredVersion; }
        public UserTokenBadVersionException(string token, long tokenVersion, long requiredVersion, Exception inner) : base(token, Resources.Services.Exception.UserTokenBadVersionException, inner) { TokenVersion = tokenVersion; RequiredVersion = requiredVersion; }
        protected UserTokenBadVersionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public long TokenVersion { get; set; }

        public long RequiredVersion { get; set; }
    }

    [Serializable]
    public class UserTokenBadFormatException : UserTokenException
    {
        public UserTokenBadFormatException() : base(Resources.Services.Exception.UserTokenBadFormatException) { }
        public UserTokenBadFormatException(string token) : base(token, Resources.Services.Exception.UserTokenBadFormatException) { }
        public UserTokenBadFormatException(string token, string message) : base(token, message) { }
        public UserTokenBadFormatException(string token, Exception inner) : base(token, Resources.Services.Exception.UserTokenBadFormatException, inner) { }
        public UserTokenBadFormatException(string token, string message, Exception inner) : base(token, message, inner) { }
        protected UserTokenBadFormatException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
