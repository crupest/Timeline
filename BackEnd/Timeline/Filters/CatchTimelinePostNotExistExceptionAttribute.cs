using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Timeline.Models.Http;
using Timeline.Services.Timeline;

namespace Timeline.Filters
{
    public class CatchTimelinePostNotExistExceptionAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            const string message = "Timeline post does not exist.";

            if (context.Exception is TimelinePostNotExistException e)
            {
                if (HttpMethods.IsGet(context.HttpContext.Request.Method))
                    context.Result = new NotFoundObjectResult(new CommonResponse(ErrorCodes.TimelineController.PostNotExist, message));
                else
                    context.Result = new BadRequestObjectResult(new CommonResponse(ErrorCodes.TimelineController.PostNotExist, message));
            }
        }
    }
}
