using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using Timeline.Models.Http;
using Timeline.Services;

namespace Timeline.Filters
{
    public class CatchEntityNotExistExceptionFilter : IExceptionFilter
    {
        private static string MakeMessage(EntityNotExistException e)
        {
            return string.Format(Resource.MessageEntityNotExist, e.EntityType.Name, e.GenerateConstraintString());
        }

        private static CommonResponse MakeCommonResponse(EntityNotExistException e)
        {
            return new CommonResponse(e.EntityType.NotExistErrorCode, MakeMessage(e));
        }

        public void OnException(ExceptionContext context)
        {
            if (context.Exception is EntityNotExistException e)
            {
                if (HttpMethods.IsGet(context.HttpContext.Request.Method))
                {
                    context.Result = new NotFoundObjectResult(MakeCommonResponse(e));
                }
                else if (HttpMethods.IsDelete(context.HttpContext.Request.Method))
                {
                    if (context.ActionDescriptor.EndpointMetadata.OfType<NotEntityDeleteAttribute>().Any())
                    {
                        context.Result = new BadRequestObjectResult(MakeCommonResponse(e));
                    }
                    else
                    {
                        context.Result = new OkObjectResult(CommonDeleteResponse.NotExist());
                    }
                }
                else
                {
                    context.Result = new BadRequestObjectResult(MakeCommonResponse(e));
                }
            }
        }
    }
}
