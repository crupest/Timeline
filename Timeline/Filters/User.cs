using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Timeline.Auth;
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
                public static class User // bbb = 101
                {
                    public const int NotExist = 11010101;

                    public const int NotSelfOrAdminForbid = 11010201;
                }
            }
        }
    }
}

namespace Timeline.Filters
{
    public class SelfOrAdminAttribute : ActionFilterAttribute
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<SelfOrAdminAttribute>>();

            var user = context.HttpContext.User;

            if (user == null)
            {
                logger.LogError(LogSelfOrAdminNoUser);
                return;
            }

            if (context.ModelState.TryGetValue("username", out var model))
            {
                if (model.RawValue is string username)
                {
                    if (!user.IsAdministrator() && user.Identity.Name != username)
                    {
                        context.Result = new ObjectResult(
                            new CommonResponse(ErrorCodes.Http.Filter.User.NotSelfOrAdminForbid, MessageSelfOrAdminForbid))
                        { StatusCode = StatusCodes.Status403Forbidden };
                    }
                }
                else
                {
                    logger.LogError(LogSelfOrAdminUsernameNotString);
                }
            }
            else
            {
                logger.LogError(LogSelfOrAdminNoUsername);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class CatchUserNotExistExceptionAttribute : ExceptionFilterAttribute
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "ASP.Net already checked.")]
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is UserNotExistException)
            {
                var body = new CommonResponse(ErrorCodes.Http.Filter.User.NotExist, MessageUserNotExist);

                if (context.HttpContext.Request.Method == "GET")
                    context.Result = new NotFoundObjectResult(body);
                else
                    context.Result = new BadRequestObjectResult(body);
            }
        }
    }
}
