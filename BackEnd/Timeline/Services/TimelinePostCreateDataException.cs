namespace Timeline.Services
{
    [System.Serializable]
    public class TimelinePostCreateDataException : System.Exception
    {
        public TimelinePostCreateDataException() { }
        public TimelinePostCreateDataException(string message) : base(message) { }
        public TimelinePostCreateDataException(string message, System.Exception inner) : base(message, inner) { }
        public TimelinePostCreateDataException(long index, string? message, System.Exception? inner = null) : base($"Data at index {index} is invalid.{(message is null ? "" : " " + message)}", inner) { Index = index; }
        protected TimelinePostCreateDataException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public long Index { get; }
    }
}
