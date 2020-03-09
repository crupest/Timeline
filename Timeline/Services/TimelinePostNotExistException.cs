using System;

namespace TimelineApp.Services
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

        public TimelinePostNotExistException(string timelineName, long id, bool isDelete = false) : base(Resources.Services.Exception.TimelinePostNotExistException) { TimelineName = timelineName; Id = id; IsDelete = isDelete; }

        public TimelinePostNotExistException(string timelineName, long id, bool isDelete, string message) : base(message) { TimelineName = timelineName; Id = id; IsDelete = isDelete; }

        public TimelinePostNotExistException(string timelineName, long id, bool isDelete, string message, Exception inner) : base(message, inner) { TimelineName = timelineName; Id = id; IsDelete = isDelete; }

        public string TimelineName { get; set; } = "";
        public long Id { get; set; }
        /// <summary>
        /// True if the post is deleted. False if the post does not exist at all.
        /// </summary>
        public bool IsDelete { get; set; } = false;
    }
}
