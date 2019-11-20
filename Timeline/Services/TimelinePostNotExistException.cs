using System;

namespace Timeline.Services
{
    [Serializable]
    public class TimelinePostNotExistException : Exception
    {
        public TimelinePostNotExistException() { }
        public TimelinePostNotExistException(string message) : base(message) { }
        public TimelinePostNotExistException(string message, Exception inner) : base(message, inner) { }
        protected TimelinePostNotExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public TimelinePostNotExistException(long id) : base(Resources.Services.Exception.TimelinePostNotExistException) { Id = id; }

        public TimelinePostNotExistException(long id, string message) : base(message) { Id = id; }

        public TimelinePostNotExistException(long id, string message, Exception inner) : base(message, inner) { Id = id; }

        public long Id { get; set; }
    }
}
