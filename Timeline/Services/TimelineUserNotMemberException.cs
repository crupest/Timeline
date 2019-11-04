using System;

namespace Timeline.Services
{
    [Serializable]
    public class TimelineUserNotMemberException : Exception
    {
        public TimelineUserNotMemberException() : base(Resources.Services.Exception.TimelineUserNotMemberException) { }
        public TimelineUserNotMemberException(string message) : base(message) { }
        public TimelineUserNotMemberException(string message, Exception inner) : base(message, inner) { }
        protected TimelineUserNotMemberException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
