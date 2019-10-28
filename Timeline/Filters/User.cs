using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using Timeline.Models.Http;

namespace Timeline
{
    public static partial class ErrorCodes
    {
        public static partial class Http
        {
            public static partial class Filter // bxx = 1xx
            {
                public static class User // bbb = 101
                {
                    public const int NotExist = 11010001;
                }

            }
        }
    }
}

namespace Timeline.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class CatchUserNotExistExceptionAttribute : ExceptionFilterAttribute
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "ASP.Net already checked.")]
        public override void OnException(ExceptionContext context)
        {
            var body = new CommonResponse(
                ErrorCodes.Http.Filter.User.NotExist,
                Resources.Filters.MessageUserNotExist);

            if (context.HttpContext.Request.Method == "GET")
                context.Result = new NotFoundObjectResult(body);
            else
                context.Result = new BadRequestObjectResult(body);
        }
    }
}
