using System;

namespace Timeline.Services.User
{
    /// <summary>
    /// The user requested does not exist.
    /// </summary>
    [Serializable]
    public class UserAlreadyExistException : EntityAlreadyExistException
    {
        public UserAlreadyExistException() : this(null, null, null) { }
        public UserAlreadyExistException(object? entity) : this(entity, null, null) { }
        public UserAlreadyExistException(object? entity, Exception? inner) : this(entity, null, inner) { }
        public UserAlreadyExistException(object? entity, string? message, Exception? inner)
            : base(EntityNames.User, entity, message ?? Resource.ExceptionUserAlreadyExist, inner)
        {

        }

        protected UserAlreadyExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
