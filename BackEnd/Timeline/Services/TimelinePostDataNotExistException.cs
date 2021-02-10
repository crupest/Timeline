using System;

namespace Timeline.Services
{
    [Serializable]
    public class TimelinePostDataNotExistException : Exception
    {
        public TimelinePostDataNotExistException() : this(null, null) { }
        public TimelinePostDataNotExistException(string? message) : this(message, null) { }
        public TimelinePostDataNotExistException(string? message, Exception? inner) : base(message, inner) { }
        protected TimelinePostDataNotExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
