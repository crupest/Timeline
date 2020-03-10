using System;

namespace Timeline.Services
{
    [Serializable]
    public class TimelineNotExistException : Exception
    {
        public TimelineNotExistException() : base(Resources.Services.Exception.TimelineNotExistException) { }
        public TimelineNotExistException(string name)
            : base(Resources.Services.Exception.TimelineNotExistException) { Name = name; }
        public TimelineNotExistException(string name, Exception inner)
            : base(Resources.Services.Exception.TimelineNotExistException, inner) { Name = name; }
        protected TimelineNotExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public string? Name { get; set; }
    }
}
