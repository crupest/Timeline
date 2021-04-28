using Microsoft.AspNetCore.Mvc;
using System;
using Timeline.Auth;
using Timeline.Services.User;

namespace Timeline.Controllers
{
    public static class ControllerAuthExtensions
    {
        public static bool UserHasPermission(this ControllerBase controller, UserPermission permission)
        {
            return controller.User.HasPermission(permission);
        }

        public static long GetUserId(this ControllerBase controller)
        {
            return controller.GetOptionalUserId() ?? throw new InvalidOperationException(Resource.ExceptionNoUserId);
        }

        public static long? GetOptionalUserId(this ControllerBase controller)
        {
            return controller.User.GetUserId();
        }
    }
}
