using System;

namespace Timeline
{

    [Serializable]
    public class InvalidBranchException : Exception
    {
        public InvalidBranchException() : base(Resources.Common.ExceptionInvalidBranch) { }
        public InvalidBranchException(string message) : base(message) { }
        public InvalidBranchException(string message, Exception inner) : base(message, inner) { }
        protected InvalidBranchException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
