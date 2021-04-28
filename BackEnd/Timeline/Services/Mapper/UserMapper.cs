using Microsoft.AspNetCore.Mvc;
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
                    self: urlHelper.ActionLink(nameof(UserController.Get), nameof(UserController)[0..^nameof(Controller).Length], new { entity.Username }),
                    avatar: urlHelper.ActionLink(nameof(UserAvatarController.Get), nameof(UserAvatarController)[0..^nameof(Controller).Length], new { entity.Username }),
                    timeline: urlHelper.ActionLink(nameof(TimelineController.TimelineGet), nameof(TimelineController)[0..^nameof(Controller).Length], new { timeline = "@" + entity.Username })
                )
            );
        }
    }
}
