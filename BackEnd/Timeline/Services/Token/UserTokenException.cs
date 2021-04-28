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
}
