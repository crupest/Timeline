using System;

namespace Timeline.Services.Exceptions
{
    [Serializable]
    public class TimelinePostNoDataException : Exception
    {
        public TimelinePostNoDataException() : this(null, null) { }
        public TimelinePostNoDataException(string? message) : this(message, null) { }
        public TimelinePostNoDataException(string? message, Exception? inner) : base(Resources.Services.Exceptions.TimelineNoDataException.AppendAdditionalMessage(message), inner) { }
        protected TimelinePostNoDataException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
