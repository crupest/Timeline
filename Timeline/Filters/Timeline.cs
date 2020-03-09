using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TimelineApp.Models.Http;
using TimelineApp.Services;

namespace TimelineApp.Filters
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
                    context.Result = new NotFoundObjectResult(ErrorResponse.TimelineCommon.NotExist());
                }
            }
        }
    }
}
