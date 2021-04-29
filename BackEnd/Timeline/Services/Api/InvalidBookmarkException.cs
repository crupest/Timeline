using System;

namespace Timeline.Services.Api
{
    [Serializable]
    public class InvalidBookmarkException : Exception
    {
        public InvalidBookmarkException() { }
        public InvalidBookmarkException(string message) : base(message) { }
        public InvalidBookmarkException(string message, Exception inner) : base(message, inner) { }
        protected InvalidBookmarkException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
