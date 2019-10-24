using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
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
                var localizerFactory = context.HttpContext.RequestServices.GetRequiredService<IStringLocalizerFactory>();
                context.Result = new BadRequestObjectResult(HeaderErrorResponse.MissingContentType(localizerFactory));
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
                var localizerFactory = context.HttpContext.RequestServices.GetRequiredService<IStringLocalizerFactory>();
                context.Result = new BadRequestObjectResult(HeaderErrorResponse.MissingContentLength(localizerFactory));
                return;
            }

            if (RequireNonZero && context.HttpContext.Request.ContentLength.Value == 0)
            {
                var localizerFactory = context.HttpContext.RequestServices.GetRequiredService<IStringLocalizerFactory>();
                context.Result = new BadRequestObjectResult(HeaderErrorResponse.ZeroContentLength(localizerFactory));
                return;
            }
        }
    }
}
