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
        public UsernameBadFormatException(string message) : base(message) { }
        public UsernameBadFormatException(string message, Exception inner) : base(message, inner) { }

        public UsernameBadFormatException(string username, string validationMessage) : this() { Username = username; ValidationMessage = validationMessage; }

        public UsernameBadFormatException(string username, string validationMessage, string message) : this(message) { Username = username; ValidationMessage = validationMessage; }

        protected UsernameBadFormatException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Username of bad format.
        /// </summary>
        public string Username { get; private set; } = "";

        public string ValidationMessage { get; private set; } = "";
    }
}
