using System;
using Microsoft.AspNetCore.Mvc;
using Timeline.Auth;
using Timeline.Services.User;

namespace Timeline.Controllers.V2
{
    public class V2ControllerBase : ControllerBase
    {
        #region auth
        protected bool UserHasPermission(UserPermission permission)
        {
            return User.HasPermission(permission);
        }

        protected long? GetOptionalAuthUserId()
        {
            return User.GetOptionalUserId();
        }

        protected long GetAuthUserId()
        {
            return GetOptionalAuthUserId() ?? throw new InvalidOperationException(Resource.ExceptionNoUserId);
        }
        #endregion
    }
}

