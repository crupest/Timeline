using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Timeline.Controllers;
using Timeline.Entities;
using Timeline.Models.Http;
using Timeline.Services.User;

namespace Timeline.Services.Mapper
{
    public class UserMapper : IMapper<UserEntity, HttpUser>
    {
        private readonly DatabaseContext _database;
        private readonly IUserPermissionService _userPermissionService;

        public UserMapper(DatabaseContext database, IUserPermissionService userPermissionService)
        {
            _database = database;
            _userPermissionService = userPermissionService;
        }

        public async Task<HttpUser> MapAsync(UserEntity entity, IUrlHelper urlHelper, ClaimsPrincipal? user)
        {
            return new HttpUser(
                uniqueId: entity.UniqueId,
                username: entity.Username,
                nickname: string.IsNullOrEmpty(entity.Nickname) ? entity.Username : entity.Nickname,
                permissions: (await _userPermissionService.GetPermissionsOfUserAsync(entity.Id, false)).ToStringList(),
                links: new HttpUserLinks(
                    self: urlHelper.ActionLink("Get", "UserV2", new { username = entity.Username }) ?? throw new Exception("Failed to generate link for user self."),
                    avatar: urlHelper.ActionLink("Get", "UserAvatarV2", new { username = entity.Username }) ?? throw new Exception("Failed to generate link for user avatar.")
                )
            );
        }
    }
}
