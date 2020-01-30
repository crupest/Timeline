using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Timeline.Auth;
using System;

namespace Timeline.Controllers
{
    public static class ControllerAuthExtensions
    {
        public static bool IsAdministrator(this ControllerBase controller)
        {
            return controller.User != null && controller.User.IsAdministrator();
        }

        public static long GetUserId(this ControllerBase controller)
        {
            if (controller.User == null)
                throw new InvalidOperationException("Failed to get user id because User is null.");

            var claim = controller.User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
                throw new InvalidOperationException("Failed to get user id because User has no NameIdentifier claim.");

            if (long.TryParse(claim.Value, out var value))
                return value;

            throw new InvalidOperationException("Failed to get user id because NameIdentifier claim is not a number.");
        }

        public static long? GetOptionalUserId(this ControllerBase controller)
        {
            if (controller.User == null)
                return null;

            var claim = controller.User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
                return null;

            if (long.TryParse(claim.Value, out var value))
                return value;

            throw new InvalidOperationException("Failed to get user id because NameIdentifier claim is not a number.");
        }
    }
}
