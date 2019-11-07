using System;
using System.Globalization;

namespace Timeline.Services
{
    [Serializable]
    public class TimelineMemberOperationUserException : Exception
    {
        public enum MemberOperation
        {
            Add,
            Remove
        }

        public TimelineMemberOperationUserException() : base(Resources.Services.Exception.TimelineMemberOperationException) { }
        public TimelineMemberOperationUserException(string message) : base(message) { }
        public TimelineMemberOperationUserException(string message, Exception inner) : base(message, inner) { }
        protected TimelineMemberOperationUserException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public TimelineMemberOperationUserException(int index, MemberOperation operation, string username, Exception inner)
            : base(MakeMessage(operation, index), inner) { Operation = operation; Index = index; Username = username; }

        private static string MakeMessage(MemberOperation operation, int index) => string.Format(CultureInfo.CurrentCulture,
            Resources.Services.Exception.TimelineMemberOperationExceptionDetail, operation, index);

        public MemberOperation? Operation { get; set; }

        /// <summary>
        /// The index of the member on which the operation failed.
        /// </summary>
        public int? Index { get; set; }

        public string? Username { get; set; }
    }
}
