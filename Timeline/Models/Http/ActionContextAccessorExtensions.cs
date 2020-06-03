using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;

namespace Timeline.Models.Http
{
    public static class ActionContextAccessorExtensions
    {
        public static ActionContext AssertActionContextForUrlFill(this IActionContextAccessor accessor)
        {
            return accessor.ActionContext ?? throw new InvalidOperationException(Resources.Models.Http.Exception.ActionContextNull);
        }
    }
}
