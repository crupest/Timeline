using Microsoft.AspNetCore.Mvc;
using Timeline.Models.Http;

namespace Timeline.Controllers
{
    public static class ActionResultControllerExtensions
    {
        public static BadRequestObjectResult BadRequestWithCodeAndMessage(this ControllerBase controller, int code, string message)
        {
            return controller.BadRequest(new CommonResponse(code, message));
        }
    }
}
