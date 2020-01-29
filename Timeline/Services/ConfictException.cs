using System;

namespace Timeline.Services
{
    /// <summary>
    /// Thrown when a resource already exists and conflicts with the given resource.
    /// </summary>
    /// <remarks>
    /// For example a username already exists and conflicts with the given username.
    /// </remarks>
    [Serializable]
    public class ConfictException : Exception
    {
        public ConfictException() : base(Resources.Services.Exception.ConfictException) { }
        public ConfictException(string message) : base(message) { }
        public ConfictException(string message, Exception inner) : base(message, inner) { }
        protected ConfictException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
