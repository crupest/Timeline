using System;

namespace Timeline.Services.User
{
    [Serializable]
    public class BadPasswordException : Exception
    {
        public BadPasswordException() : this(null, null, null) { }
        public BadPasswordException(string? badPassword) : this(badPassword, null, null) { }
        public BadPasswordException(string? badPassword, Exception? inner) : this(badPassword, null, inner) { }
        public BadPasswordException(string? badPassword, string? message, Exception? inner) : base(message ?? Resource.ExceptionBadPassword, inner)
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
