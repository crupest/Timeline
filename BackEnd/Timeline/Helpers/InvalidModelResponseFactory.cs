using Microsoft.AspNetCore.Mvc;
using System.Text;
using Timeline.Models.Http;

namespace Timeline.Helpers
{
    public static class InvalidModelResponseFactory
    {
        public static IActionResult Factory(ActionContext context)
        {
            if (context.HttpContext.Request.Path.StartsWithSegments("/api/v2"))
            {
                return new UnprocessableEntityObjectResult(new ErrorResponse(ErrorResponse.InvalidRequest, "Request is of bad format."));
            }

            var modelState = context.ModelState;

            var messageBuilder = new StringBuilder();
            foreach (var model in modelState)
                foreach (var error in model.Value.Errors)
                {
                    messageBuilder.Append(model.Key);
                    messageBuilder.Append(" : ");
                    messageBuilder.AppendLine(error.ErrorMessage);
                }

            return new BadRequestObjectResult(new CommonResponse(ErrorCodes.Common.InvalidModel, $"Request format is bad. {messageBuilder}"));
        }
    }
}
