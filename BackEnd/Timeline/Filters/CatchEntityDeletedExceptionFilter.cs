using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Timeline.Services;

namespace Timeline.Filters
{
    public class CatchEntityDeletedExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is EntityDeletedException)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status410Gone);
            }
        }
    }
}
