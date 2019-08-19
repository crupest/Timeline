using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Timeline.Models.Http;

namespace Timeline.Filters
{
    public class RequireContentTypeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Request.ContentType == null)
            {
                context.Result = new BadRequestObjectResult(CommonResponse.MissingContentType());
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

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Request.ContentLength == null)
            {
                context.Result = new BadRequestObjectResult(CommonResponse.MissingContentLength());
                return;
            }

            if (RequireNonZero && context.HttpContext.Request.ContentLength.Value == 0)
            {
                context.Result = new BadRequestObjectResult(CommonResponse.ZeroContentLength());
                return;
            }
        }
    }
}
