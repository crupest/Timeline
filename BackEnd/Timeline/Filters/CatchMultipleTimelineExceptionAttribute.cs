using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Timeline.Models.Http;
using Timeline.Services.Timeline;

namespace Timeline.Filters
{
    public class CatchMultipleTimelineExceptionAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is MultipleTimelineException)
            {
                context.Result = new BadRequestObjectResult(new CommonResponse(ErrorCodes.TimelineController.MultipleTimelineWithSameName, Resource.MessageMultipleTimeline));
            }
        }
    }
}

