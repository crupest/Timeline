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
    public class ConflictException : Exception
    {
        public ConflictException() : base(Resources.Services.Exception.ConflictException) { }
        public ConflictException(string message) : base(message) { }
        public ConflictException(string message, Exception inner) : base(message, inner) { }
        protected ConflictException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
