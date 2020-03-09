using Microsoft.AspNetCore.Mvc;
using System.Text;
using TimelineApp.Models.Http;

namespace TimelineApp.Helpers
{
    public static class InvalidModelResponseFactory
    {
        public static IActionResult Factory(ActionContext context)
        {
            var modelState = context.ModelState;

            var messageBuilder = new StringBuilder();
            foreach (var model in modelState)
                foreach (var error in model.Value.Errors)
                {
                    messageBuilder.Append(model.Key);
                    messageBuilder.Append(" : ");
                    messageBuilder.AppendLine(error.ErrorMessage);
                }

            return new BadRequestObjectResult(ErrorResponse.Common.CustomMessage_InvalidModel(messageBuilder.ToString()));
        }
    }
}
