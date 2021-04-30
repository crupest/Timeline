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
                context.Result = new BadRequestObjectResult(
                    new CommonResponse(ErrorCodes.Common.Content.TooBig,
                        string.Format(Resource.MessageContentLengthTooBig, MaxByteLength + "B")));
            }
        }
    }
}
