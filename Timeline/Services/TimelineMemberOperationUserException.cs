using System;
using System.Globalization;

namespace Timeline.Services
{
    [Serializable]
    public class TimelineMemberOperationUserException : Exception
    {
        public TimelineMemberOperationUserException() : base(Resources.Services.Exception.TimelineMemberOperationException) { }
        public TimelineMemberOperationUserException(string message) : base(message) { }
        public TimelineMemberOperationUserException(string message, Exception inner) : base(message, inner) { }
        protected TimelineMemberOperationUserException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public TimelineMemberOperationUserException(int index, string username, Exception inner) : base(MakeIndexMessage(index), inner) { Index = index; Username = username; }

        private static string MakeIndexMessage(int index) => string.Format(CultureInfo.CurrentCulture,
            Resources.Services.Exception.TimelineMemberOperationExceptionIndex, index);

        /// <summary>
        /// The index of the member on which the operation failed.
        /// </summary>
        public int? Index { get; set; }

        public string? Username { get; set; }
    }
}
