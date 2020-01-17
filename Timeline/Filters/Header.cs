using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Timeline.Models.Http;

namespace Timeline.Filters
{
    public class RequireContentTypeAttribute : ActionFilterAttribute
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Request.ContentType == null)
            {
                context.Result = new BadRequestObjectResult(ErrorResponse.Common.Header.ContentType_Missing());
            }
        }
    }

    public class RequireContentLengthAttribute : ActionFilterAttribute
    {
        public RequireContentLengthAttribute()
        : this(true)
        {

        }

        public RequireContentLengthAttribute(bool requireNonZero)
        {
            RequireNonZero = requireNonZero;
        }

        public bool RequireNonZero { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Request.ContentLength == null)
            {
                context.Result = new BadRequestObjectResult(ErrorResponse.Common.Header.ContentLength_Missing());
                return;
            }

            if (RequireNonZero && context.HttpContext.Request.ContentLength.Value == 0)
            {
                context.Result = new BadRequestObjectResult(ErrorResponse.Common.Header.ContentLength_Zero());
                return;
            }
        }
    }
}
