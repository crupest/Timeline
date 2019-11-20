using System;

namespace Timeline.Services
{
    [Serializable]
    public class TimelineNameBadFormatException : Exception
    {
        public TimelineNameBadFormatException()
            : base(Resources.Services.Exception.TimelineNameBadFormatException) { }
        public TimelineNameBadFormatException(string name)
            : base(Resources.Services.Exception.TimelineNameBadFormatException) { Name = name; }
        public TimelineNameBadFormatException(string name, Exception inner)
            : base(Resources.Services.Exception.TimelineNameBadFormatException, inner) { Name = name; }

        protected TimelineNameBadFormatException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public string? Name { get; set; }
    }
}
