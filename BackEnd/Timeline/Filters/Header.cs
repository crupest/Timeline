using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Timeline.Models.Http;

namespace Timeline.Filters
{
    /// <summary>
    /// Restrict max content length.
    /// </summary>
    public class MaxContentLengthFilter : IResourceFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxByteLength">Max length.</param>
        public MaxContentLengthFilter(long maxByteLength)
        {
            MaxByteLength = maxByteLength;
        }

        /// <summary>
        /// Max length.
        /// </summary>
        public long MaxByteLength { get; set; }

        /// <inheritdoc/>
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
        }

        /// <inheritdoc/>
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            var contentLength = context.HttpContext.Request.ContentLength;
            if (contentLength != null && contentLength > MaxByteLength)
            {
                context.Result = new BadRequestObjectResult(ErrorResponse.Common.Content.TooBig(MaxByteLength + "B"));
            }
        }
    }

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
