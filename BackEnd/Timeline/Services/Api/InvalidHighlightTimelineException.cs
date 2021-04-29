using System;

namespace Timeline.Services.Api
{
    [Serializable]
    public class InvalidHighlightTimelineException : Exception
    {
        public InvalidHighlightTimelineException() { }
        public InvalidHighlightTimelineException(string message) : base(message) { }
        public InvalidHighlightTimelineException(string message, Exception inner) : base(message, inner) { }
        protected InvalidHighlightTimelineException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
