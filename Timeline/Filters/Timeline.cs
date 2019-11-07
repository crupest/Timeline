using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Timeline.Models.Http;
using Timeline.Services;
using static Timeline.Resources.Filters;

namespace Timeline
{
    public static partial class ErrorCodes
    {
        public static partial class Http
        {
            public static partial class Filter // bxx = 1xx
            {
                public static class Timeline // bbb = 102
                {
                    public const int UserNotExist = 11020101;
                    public const int NameNotExist = 11020102;
                }
            }
        }
    }
}

namespace Timeline.Filters
{
    public class CatchTimelineNotExistExceptionAttribute : ExceptionFilterAttribute
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is TimelineNotExistException e)
            {
                if (e.InnerException is UserNotExistException)
                {
                    context.Result = new BadRequestObjectResult(
                        new CommonResponse(ErrorCodes.Http.Filter.Timeline.UserNotExist, MessageTimelineNotExistUser));
                }
                else
                {
                    context.Result = new BadRequestObjectResult(
                        new CommonResponse(ErrorCodes.Http.Filter.Timeline.NameNotExist, MessageTimelineNotExist));
                }
            }
        }
    }
}
