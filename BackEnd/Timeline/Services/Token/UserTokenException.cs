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


    [Serializable]
    public class UserTokenUserNotExistException : UserTokenException
    {
        public UserTokenUserNotExistException() : base(Resource.ExceptionUserTokenUserNotExist) { }
        public UserTokenUserNotExistException(string token) : base(token, Resource.ExceptionUserTokenUserNotExist) { }
        public UserTokenUserNotExistException(string token, Exception inner) : base(token, Resource.ExceptionUserTokenUserNotExist, inner) { }

        protected UserTokenUserNotExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class UserTokenBadFormatException : UserTokenException
    {
        public UserTokenBadFormatException() : base(Resource.ExceptionUserTokenBadFormat) { }
        public UserTokenBadFormatException(string token) : base(token, Resource.ExceptionUserTokenBadFormat) { }
        public UserTokenBadFormatException(string token, string message) : base(token, message) { }
        public UserTokenBadFormatException(string token, Exception inner) : base(token, Resource.ExceptionUserTokenBadFormat, inner) { }
        public UserTokenBadFormatException(string token, string message, Exception inner) : base(token, message, inner) { }
        protected UserTokenBadFormatException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
