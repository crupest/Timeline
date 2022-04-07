using System;
namespace Timeline.Services.Timeline
{
    /// <summary>
    /// Thrown when call <see cref="ITimelineService.GetTimelineIdByNameAsync(string)"/> and multiple timelines have that same name.
    /// </summary>
    [Serializable]
    public class MultipleTimelineException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:MultipleTimelineException"/> class
        /// </summary>
        public MultipleTimelineException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MultipleTimelineException"/> class
        /// </summary>
        /// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
        public MultipleTimelineException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MultipleTimelineException"/> class
        /// </summary>
        /// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
        /// <param name="inner">The exception that is the cause of the current exception. </param>
        public MultipleTimelineException(string message, System.Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MultipleTimelineException"/> class
        /// </summary>
        /// <param name="context">The contextual information about the source or destination.</param>
        /// <param name="info">The object that holds the serialized object data.</param>
        protected MultipleTimelineException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}

