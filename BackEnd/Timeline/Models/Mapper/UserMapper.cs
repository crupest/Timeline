using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Timeline.Controllers;
using Timeline.Entities;
using Timeline.Models.Http;
using Timeline.Services;

namespace Timeline.Models.Mapper
{
    public static class UserMapper
    {
        public static HttpUser MapToHttp(this UserEntity entity, IUrlHelper urlHelper)
        {
            return new HttpUser(
                uniqueId: entity.UniqueId,
                username: entity.Username,
                nickname: string.IsNullOrEmpty(entity.Nickname) ? entity.Username : entity.Nickname,
                permissions: MapPermission(entity),
                links: new HttpUserLinks(
                    self: urlHelper.ActionLink(nameof(UserController.Get), nameof(UserController)[0..^nameof(Controller).Length], new { entity.Username }),
                    avatar: urlHelper.ActionLink(nameof(UserAvatarController.Get), nameof(UserAvatarController)[0..^nameof(Controller).Length], new { entity.Username }),
                    timeline: urlHelper.ActionLink(nameof(TimelineController.TimelineGet), nameof(TimelineController)[0..^nameof(Controller).Length], new { timeline = "@" + entity.Username })
                )
            );
        }

        public static List<HttpUser> MapToHttp(this List<UserEntity> entities, IUrlHelper urlHelper)
        {
            return entities.Select(e => e.MapToHttp(urlHelper)).ToList();
        }

        private static List<string> MapPermission(UserEntity entity)
        {
            return entity.Permissions.Select(p => p.Permission).ToList();
        }
    }
}
