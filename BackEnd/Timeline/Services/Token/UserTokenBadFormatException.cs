using System;

namespace Timeline.Services.Token
{
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
