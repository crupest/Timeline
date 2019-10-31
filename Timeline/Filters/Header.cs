using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Timeline.Models.Http;
using static Timeline.Resources.Filters;

namespace Timeline
{
    public static partial class ErrorCodes
    {
        public static partial class Http
        {
            public static partial class Filter // bxx = 1xx
            {
                public static partial class Header // bbb = 100
                {
                    public static class ContentType // cc = 01
                    {
                        public const int Missing = 11000101; // dd = 01
                    }

                    public static class ContentLength // cc = 02
                    {
                        public const int Missing = 11000201; // dd = 01
                        public const int Zero = 11000202; // dd = 02
                    }
                }
            }

        }
    }
}

namespace Timeline.Filters
{
    public class RequireContentTypeAttribute : ActionFilterAttribute
    {
        internal static CommonResponse CreateResponse()
        {
            return new CommonResponse(
                ErrorCodes.Http.Filter.Header.ContentType.Missing,
                MessageHeaderContentTypeMissing);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Request.ContentType == null)
            {
                context.Result = new BadRequestObjectResult(CreateResponse());
            }
        }
    }

    public class RequireContentLengthAttribute : ActionFilterAttribute
    {
        internal static CommonResponse CreateMissingResponse()
        {
            return new CommonResponse(
                ErrorCodes.Http.Filter.Header.ContentLength.Missing,
                MessageHeaderContentLengthMissing);
        }

        internal static CommonResponse CreateZeroResponse()
        {
            return new CommonResponse(
                ErrorCodes.Http.Filter.Header.ContentLength.Zero,
                MessageHeaderContentLengthZero);
        }

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
                context.Result = new BadRequestObjectResult(CreateMissingResponse());
                return;
            }

            if (RequireNonZero && context.HttpContext.Request.ContentLength.Value == 0)
            {
                context.Result = new BadRequestObjectResult(CreateZeroResponse());
                return;
            }
        }
    }
}
