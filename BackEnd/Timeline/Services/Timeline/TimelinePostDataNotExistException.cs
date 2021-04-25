using System;

namespace Timeline.Services.Timeline
{
    [Serializable]
    public class TimelinePostDataNotExistException : Exception
    {
        public TimelinePostDataNotExistException() : this(null, null) { }
        public TimelinePostDataNotExistException(string? message) : this(message, null) { }
        public TimelinePostDataNotExistException(string? message, Exception? inner) : base(message, inner) { }
        public TimelinePostDataNotExistException(long timelineId, long postId, long dataIndex, string? message = null, Exception? inner = null) : base(message, inner)
        {
            TimelineId = timelineId;
            PostId = postId;
            DataIndex = dataIndex;
        }
        protected TimelinePostDataNotExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public long TimelineId { get; set; }
        public long PostId { get; set; }
        public long DataIndex { get; set; }
    }
}
