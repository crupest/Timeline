using Microsoft.AspNetCore.Mvc;
using System.Text;
using Timeline.Models.Http;

namespace Timeline.Helpers
{
    public static class InvalidModelResponseFactory
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
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
