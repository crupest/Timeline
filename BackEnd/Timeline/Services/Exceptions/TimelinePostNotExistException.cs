using System;
using System.Globalization;

namespace Timeline.Services.Exceptions
{
    [Serializable]
    public class TimelinePostNotExistException : EntityNotExistException
    {
        public TimelinePostNotExistException() : this(null, null, false, null, null) { }
        [Obsolete("This has no meaning.")]
        public TimelinePostNotExistException(string? message) : this(message, null) { }
        [Obsolete("This has no meaning.")]
        public TimelinePostNotExistException(string? message, Exception? inner) : this(null, null, false, message, inner) { }
        protected TimelinePostNotExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public TimelinePostNotExistException(string? timelineName, long? id, bool isDelete, string? message = null, Exception? inner = null) : base(EntityNames.TimelinePost, null, MakeMessage(timelineName, id, isDelete).AppendAdditionalMessage(message), inner) { TimelineName = timelineName; Id = id; IsDelete = isDelete; }

        private static string MakeMessage(string? timelineName, long? id, bool isDelete)
        {
            return string.Format(CultureInfo.InvariantCulture, isDelete ? Resources.Services.Exceptions.TimelinePostNotExistExceptionDeleted : Resources.Services.Exceptions.TimelinePostNotExistException, timelineName ?? "", id);
        }

        public string? TimelineName { get; set; }
        public long? Id { get; set; }

        /// <summary>
        /// True if the post is deleted. False if the post does not exist at all.
        /// </summary>
        public bool IsDelete { get; set; }
    }
}
