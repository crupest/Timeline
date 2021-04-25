using System;

namespace Timeline.Services.User
{

    [Serializable]
    public class InvalidOperationOnRootUserException : InvalidOperationException
    {
        public InvalidOperationOnRootUserException() { }
        public InvalidOperationOnRootUserException(string message) : base(message) { }
        public InvalidOperationOnRootUserException(string message, Exception inner) : base(message, inner) { }
        protected InvalidOperationOnRootUserException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
