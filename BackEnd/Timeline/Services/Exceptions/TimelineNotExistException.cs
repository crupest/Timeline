using System;
using System.Globalization;

namespace Timeline.Services.Exceptions
{
    [Serializable]
    public class TimelineNotExistException : EntityNotExistException
    {
        public TimelineNotExistException() : this((long?)null) { }
        public TimelineNotExistException(long? id) : this(id, null) { }
        public TimelineNotExistException(long? id, Exception? inner) : this(id, null, inner) { }
        public TimelineNotExistException(long? id, string? message, Exception? inner) : base(EntityNames.Timeline, null, message, inner) { TimelineId = id; }

        public TimelineNotExistException(string? timelineName) : this(timelineName, null) { }
        public TimelineNotExistException(string? timelineName, Exception? inner) : this(timelineName, null, inner) { }
        public TimelineNotExistException(string? timelineName, string? message, Exception? inner = null)
            : base(EntityNames.Timeline, null, string.Format(CultureInfo.InvariantCulture, Resources.Services.Exceptions.TimelineNotExistException, timelineName ?? "").AppendAdditionalMessage(message), inner) { TimelineName = timelineName; }

        protected TimelineNotExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public string? TimelineName { get; set; }
        public long? TimelineId { get; set; }
    }
}
