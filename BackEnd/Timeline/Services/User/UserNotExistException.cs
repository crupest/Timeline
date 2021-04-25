using System;

namespace Timeline.Services.User
{
    /// <summary>
    /// The user requested does not exist.
    /// </summary>
    [Serializable]
    public class UserNotExistException : EntityNotExistException
    {
        public UserNotExistException() : this(null, null, null, null) { }
        public UserNotExistException(string? username) : this(username, null, null, null) { }
        public UserNotExistException(string? username, Exception? inner) : this(username, null, null, inner) { }
        public UserNotExistException(long id) : this(null, id, null, null) { }
        public UserNotExistException(long id, Exception? inner) : this(null, id, null, inner) { }
        public UserNotExistException(string? username, long? id, string? message, Exception? inner)
            : base(EntityNames.User, message ?? Resource.ExceptionUserNotExist, inner)
        {
            Username = username;
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
