using System;
using Timeline.Helpers;

namespace Timeline.Services
{
    /// <summary>
    /// Thrown when the user already exists.
    /// </summary>
    [Serializable]
    public class UsernameConfictException : Exception
    {
        public UsernameConfictException() : base(Resources.Services.Exception.UsernameConfictException) { }
        public UsernameConfictException(string username) : base(Log.Format(Resources.Services.Exception.UsernameConfictException, ("Username", username))) { Username = username; }
        public UsernameConfictException(string username, string message) : base(message) { Username = username; }
        public UsernameConfictException(string message, Exception inner) : base(message, inner) { }
        protected UsernameConfictException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        /// <summary>
        /// The username that already exists.
        /// </summary>
        public string? Username { get; set; }
    }
}
