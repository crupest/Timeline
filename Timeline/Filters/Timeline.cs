using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Timeline.Models.Http;
using Timeline.Services;

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
                    context.Result = new NotFoundObjectResult(ErrorResponse.UserCommon.NotExist());
                }
                else
                {
                    context.Result = new NotFoundObjectResult(ErrorResponse.TimelineController.NotExist());
                }
            }
        }
    }
}
