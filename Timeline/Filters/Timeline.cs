using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Timeline.Models.Http;
using Timeline.Services;

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
                    context.Result = new BadRequestObjectResult(ErrorResponse.UserCommon.NotExist());
                }
                else
                {
                    throw new System.NotImplementedException();
                }
            }
        }
    }
}
