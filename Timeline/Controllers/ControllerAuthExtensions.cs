using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using TimelineApp.Auth;
using static TimelineApp.Resources.Controllers.ControllerAuthExtensions;

namespace TimelineApp.Controllers
{
    public static class ControllerAuthExtensions
    {
        public static bool IsAdministrator(this ControllerBase controller)
        {
            return controller.User != null && controller.User.IsAdministrator();
        }

        public static long GetUserId(this ControllerBase controller)
        {
            var claim = controller.User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
                throw new InvalidOperationException(ExceptionNoUserIdentifierClaim);

            if (long.TryParse(claim.Value, out var value))
                return value;

            throw new InvalidOperationException(ExceptionUserIdentifierClaimBadFormat);
        }

        public static long? GetOptionalUserId(this ControllerBase controller)
        {
            var claim = controller.User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
                return null;

            if (long.TryParse(claim.Value, out var value))
                return value;

            throw new InvalidOperationException(ExceptionUserIdentifierClaimBadFormat);
        }
    }
}
