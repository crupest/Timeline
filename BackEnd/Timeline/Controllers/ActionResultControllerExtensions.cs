using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Timeline.Models.Http;

namespace Timeline.Controllers
{
    public static class ActionResultControllerExtensions
    {
        public static ObjectResult StatusCodeWithCommonResponse(this ControllerBase controller, int statusCode, int code, string message)
        {
            return controller.StatusCode(statusCode, new CommonResponse(code, message));
        }

        public static ObjectResult ForbidWithMessage(this ControllerBase controller, string? message = null)
        {
            return controller.StatusCode(StatusCodes.Status403Forbidden, new CommonResponse(ErrorCodes.Common.Forbid, message ?? Resource.MessageForbid));
        }

        public static BadRequestObjectResult BadRequestWithCommonResponse(this ControllerBase controller, int code, string message)
        {
            return controller.BadRequest(new CommonResponse(code, message));
        }
    }
}
