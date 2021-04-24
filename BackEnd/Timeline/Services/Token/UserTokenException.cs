using System;

namespace Timeline.Services.Token
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
    public class UserTokenTimeExpiredException : UserTokenException
    {
        public UserTokenTimeExpiredException() : base(Resources.Services.Exception.UserTokenTimeExpireException) { }
        public UserTokenTimeExpiredException(string message) : base(message) { }
        public UserTokenTimeExpiredException(string message, Exception inner) : base(message, inner) { }
        public UserTokenTimeExpiredException(string token, DateTime expireTime, DateTime verifyTime) : base(token, Resources.Services.Exception.UserTokenTimeExpireException) { ExpireTime = expireTime; VerifyTime = verifyTime; }
        public UserTokenTimeExpiredException(string token, DateTime expireTime, DateTime verifyTime, Exception inner) : base(token, Resources.Services.Exception.UserTokenTimeExpireException, inner) { ExpireTime = expireTime; VerifyTime = verifyTime; }
        protected UserTokenTimeExpiredException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public DateTime ExpireTime { get; private set; }

        public DateTime VerifyTime { get; private set; }
    }

    [Serializable]
    public class UserTokenVersionExpiredException : UserTokenException
    {
        public UserTokenVersionExpiredException() : base(Resources.Services.Exception.UserTokenBadVersionException) { }
        public UserTokenVersionExpiredException(string message) : base(message) { }
        public UserTokenVersionExpiredException(string message, Exception inner) : base(message, inner) { }
        public UserTokenVersionExpiredException(string token, long tokenVersion, long requiredVersion) : base(token, Resources.Services.Exception.UserTokenBadVersionException) { TokenVersion = tokenVersion; RequiredVersion = requiredVersion; }
        public UserTokenVersionExpiredException(string token, long tokenVersion, long requiredVersion, Exception inner) : base(token, Resources.Services.Exception.UserTokenBadVersionException, inner) { TokenVersion = tokenVersion; RequiredVersion = requiredVersion; }
        protected UserTokenVersionExpiredException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public long TokenVersion { get; set; }

        public long RequiredVersion { get; set; }
    }


    [Serializable]
    public class UserTokenUserNotExistException : UserTokenException
    {
        const string message = "The owner of the token does not exist.";

        public UserTokenUserNotExistException() : base(message) { }
        public UserTokenUserNotExistException(string token) : base(token, message) { }
        public UserTokenUserNotExistException(string token, Exception inner) : base(token, message, inner) { }

        protected UserTokenUserNotExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
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
