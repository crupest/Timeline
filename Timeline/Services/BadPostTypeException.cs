using System;

namespace Timeline.Services
{
    [Serializable]
    public class BadPostTypeException : Exception
    {
        public BadPostTypeException() { }
        public BadPostTypeException(string message) : base(message) { }
        public BadPostTypeException(string message, Exception inner) : base(message, inner) { }
        public BadPostTypeException(string requestType, string requiredType) : this() { RequestType = requestType; RequiredType = requiredType; }
        public BadPostTypeException(string requestType, string requiredType, string message) : this(message) { RequestType = requestType; RequiredType = requiredType; }
        public BadPostTypeException(string requestType, string requiredType, string message, Exception inner) : this(message, inner) { RequestType = requestType; RequiredType = requiredType; }
        protected BadPostTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public string RequestType { get; set; } = "";
        public string RequiredType { get; set; } = "";
    }
}
