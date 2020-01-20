using System;

namespace Timeline.Services
{
    /// <summary>
    /// Thrown when username is of bad format.
    /// </summary>
    [Serializable]
    public class UsernameBadFormatException : Exception
    {
        public UsernameBadFormatException() : base(Resources.Services.Exception.UsernameBadFormatException) { }
        public UsernameBadFormatException(string username) : this() { Username = username; }
        public UsernameBadFormatException(string username, Exception inner) : base(Resources.Services.Exception.UsernameBadFormatException, inner) { Username = username; }

        public UsernameBadFormatException(string username, string message) : base(message) { Username = username; }
        public UsernameBadFormatException(string username, string message, Exception inner) : base(message, inner) { Username = username; }

        protected UsernameBadFormatException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Username of bad format.
        /// </summary>
        public string Username { get; private set; } = "";
    }
}
