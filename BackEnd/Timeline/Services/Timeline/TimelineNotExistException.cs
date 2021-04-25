using System;

namespace Timeline.Services.Timeline
{
    [Serializable]
    public class TimelineNotExistException : EntityNotExistException
    {
        public TimelineNotExistException() : this(null, null, null, null) { }
        public TimelineNotExistException(long? id) : this(null, id, null, null) { }
        public TimelineNotExistException(long? id, Exception? inner) : this(null, id, null, inner) { }
        public TimelineNotExistException(string? timelineName) : this(timelineName, null, null, null) { }
        public TimelineNotExistException(string? timelineName, Exception? inner) : this(timelineName, null, null, inner) { }
        public TimelineNotExistException(string? timelineName, long? timelineId, string? message, Exception? inner = null)
            : base(EntityNames.Timeline, message ?? Resource.ExceptionTimelineNotExist, inner)
        {
            TimelineId = timelineId;
            TimelineName = timelineName;
        }

        protected TimelineNotExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public string? TimelineName { get; set; }
        public long? TimelineId { get; set; }
    }
}
