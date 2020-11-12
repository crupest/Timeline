using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using Timeline.Services;

namespace Timeline.Auth
{
    public class PermissionAuthorizeAttribute : AuthorizeAttribute
    {
        public PermissionAuthorizeAttribute()
        {

        }

        public PermissionAuthorizeAttribute(params UserPermission[] permissions)
        {
            Permissions = permissions;
        }

        public UserPermission[] Permissions
        {
            get => Policy == null ? Array.Empty<UserPermission>() : Policy[PermissionPolicyProvider.PolicyPrefix.Length..].Split(',')
                .Select(s => Enum.Parse<UserPermission>(s)).ToArray();
            set
            {
                Policy = $"{PermissionPolicyProvider.PolicyPrefix}{string.Join(',', value)}";
            }
        }
    }
}
