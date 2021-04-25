using System;

namespace Timeline.Services.Timeline
{
    /// <summary>
    /// The user requested does not exist.
    /// </summary>
    [Serializable]
    public class TimelineAlreadyExistException : EntityAlreadyExistException
    {
        public TimelineAlreadyExistException() : this(null, null, null) { }
        public TimelineAlreadyExistException(object? entity) : this(entity, null, null) { }
        public TimelineAlreadyExistException(object? entity, Exception? inner) : this(entity, null, inner) { }
        public TimelineAlreadyExistException(object? entity, string? message, Exception? inner)
            : base(EntityNames.Timeline, entity, message ?? Resource.ExceptionTimelineAlreadyExist, inner)
        {

        }

        protected TimelineAlreadyExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
