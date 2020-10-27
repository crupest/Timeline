using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Timeline.Models.Http;
using Timeline.Services.Exceptions;

namespace Timeline.Filters
{
    public class CatchTimelineNotExistExceptionAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is TimelineNotExistException e)
            {
                if (e.InnerException is UserNotExistException)
                {
                    if (HttpMethods.IsGet(context.HttpContext.Request.Method))
                        context.Result = new NotFoundObjectResult(ErrorResponse.UserCommon.NotExist());
                    else
                        context.Result = new BadRequestObjectResult(ErrorResponse.UserCommon.NotExist());
                }
                else
                {
                    if (HttpMethods.IsGet(context.HttpContext.Request.Method))
                        context.Result = new NotFoundObjectResult(ErrorResponse.TimelineController.NotExist());
                    else
                        context.Result = new BadRequestObjectResult(ErrorResponse.TimelineController.NotExist());
                }
            }
        }
    }
}
