using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Timeline.Models.Http;
using Timeline.Services;

namespace Timeline.Filters
{
    public class CatchEntityAlreadyExistExceptionFilter : IExceptionFilter
    {
        private static string MakeMessage(EntityAlreadyExistException e)
        {
            return string.Format(Resource.MessageEntityAlreadyExist, e.EntityType.Name, e.GenerateConstraintString());
        }

        private static CommonResponse MakeCommonResponse(EntityAlreadyExistException e)
        {
            return new CommonResponse(e.EntityType.ConflictErrorCode, MakeMessage(e));
        }

        public void OnException(ExceptionContext context)
        {
            if (context.Exception is EntityAlreadyExistException e)
            {
                context.Result = new BadRequestObjectResult(MakeCommonResponse(e));
            }
        }
    }
}
