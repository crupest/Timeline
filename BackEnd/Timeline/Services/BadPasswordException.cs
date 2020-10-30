using System;
using Timeline.Helpers;

namespace Timeline.Services
{
    [Serializable]
    public class BadPasswordException : Exception
    {
        public BadPasswordException() : base(Resources.Services.Exception.BadPasswordException) { }
        public BadPasswordException(string message, Exception inner) : base(message, inner) { }

        public BadPasswordException(string badPassword)
            : base(Log.Format(Resources.Services.Exception.BadPasswordException, ("Bad Password", badPassword)))
        {
            Password = badPassword;
        }

        protected BadPasswordException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        /// <summary>
        /// The wrong password.
        /// </summary>
        public string? Password { get; set; }
    }
}
