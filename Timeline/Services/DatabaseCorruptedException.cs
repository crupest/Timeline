using System;

namespace Timeline.Services
{
    [Serializable]
    public class DatabaseCorruptedException : Exception
    {
        public DatabaseCorruptedException() { }
        public DatabaseCorruptedException(string message) : base(message) { }
        public DatabaseCorruptedException(string message, Exception inner) : base(message, inner) { }
        protected DatabaseCorruptedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
