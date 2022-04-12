using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Timeline.Auth;
using Timeline.Services.Mapper;
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

        #region mapper
        protected IGenericMapper GetMapper()
        {
            return HttpContext.RequestServices.GetRequiredService<IGenericMapper>();
        }

        protected async Task<T> MapAsync<T>(object o)
        {
            return await GetMapper().MapAsync<T>(o, Url, User);
        }

        protected async Task<List<T>> MapListAsync<T>(IEnumerable<object> o)
        {
            return await GetMapper().MapListAsync<T>(o, Url, User);
        }

        protected T AutoMapperMap<T>(object o)
        {
            return GetMapper().AutoMapperMap<T>(o);
        }
        #endregion
    }
}

