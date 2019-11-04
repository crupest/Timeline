using System;

namespace Timeline.Services
{
    [Serializable]
    public class TimelineAlreadyExistException : Exception
    {
        public TimelineAlreadyExistException() : base(Resources.Services.Exception.TimelineAlreadyExistException) { }
        public TimelineAlreadyExistException(string name) : base(Resources.Services.Exception.TimelineAlreadyExistException) { Name = name; }
        public TimelineAlreadyExistException(string name, Exception inner) : base(Resources.Services.Exception.TimelineAlreadyExistException, inner) { Name = name; }
        protected TimelineAlreadyExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public string? Name { get; set; }
    }
}
