using Microsoft.AspNetCore.Mvc;

namespace Timeline.Filters
{
    /// <summary>
    /// Restrict max content length.
    /// </summary>
    public class MaxContentLengthAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxByteLength">Max length.</param>
        public MaxContentLengthAttribute(long maxByteLength)
            : base(typeof(MaxContentLengthFilter))
        {
            MaxByteLength = maxByteLength;
            Arguments = new object[] { maxByteLength };
        }

        /// <summary>
        /// Max length.
        /// </summary>
        public long MaxByteLength { get; }
    }
}
