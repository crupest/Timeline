using System;
using Timeline.Helpers;

namespace Timeline.Services
{
    /// <summary>
    /// The user requested does not exist.
    /// </summary>
    [Serializable]
    public class UserNotExistException : Exception
    {
        public UserNotExistException() : base(Resources.Services.Exception.UserNotExistException) { }
        public UserNotExistException(string message, Exception inner) : base(message, inner) { }

        public UserNotExistException(string username)
            : base(Log.Format(Resources.Services.Exception.UserNotExistException, ("Username", username)))
        {
            Username = username;
        }

        public UserNotExistException(long id)
            : base(Log.Format(Resources.Services.Exception.UserNotExistException, ("Id", id)))
        {
            Id = id;
        }

        protected UserNotExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        /// <summary>
        /// The username of the user that does not exist.
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// The id of the user that does not exist.
        /// </summary>
        public long? Id { get; set; }
    }
}
