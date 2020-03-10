using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using Timeline.Controllers;

namespace Timeline.Models.Http
{
    public class UserInfo
    {
        public string Username { get; set; } = default!;
        public string Nickname { get; set; } = default!;
        public bool? Administrator { get; set; } = default!;
#pragma warning disable CA1707 // Identifiers should not contain underscores
        public UserInfoLinks _links { get; set; } = default!;
#pragma warning restore CA1707 // Identifiers should not contain underscores
    }

    public class UserInfoLinks
    {
        public string Self { get; set; } = default!;
        public string Avatar { get; set; } = default!;
        public string Timeline { get; set; } = default!;
    }

    public class UserInfoLinksValueResolver : IValueResolver<User, UserInfo, UserInfoLinks>
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;

        public UserInfoLinksValueResolver(IActionContextAccessor actionContextAccessor, IUrlHelperFactory urlHelperFactory)
        {
            _actionContextAccessor = actionContextAccessor;
            _urlHelperFactory = urlHelperFactory;
        }

        public UserInfoLinks Resolve(User source, UserInfo destination, UserInfoLinks destMember, ResolutionContext context)
        {
            if (_actionContextAccessor.ActionContext == null)
                throw new InvalidOperationException("No action context, can't fill urls.");

            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var result = new UserInfoLinks
            {
                Self = urlHelper.ActionLink(nameof(UserController.Get), nameof(UserController)[0..^nameof(Controller).Length], new { destination.Username }),
                Avatar = urlHelper.ActionLink(nameof(UserAvatarController.Get), nameof(UserAvatarController)[0..^nameof(Controller).Length], new { destination.Username }),
                Timeline = urlHelper.ActionLink(nameof(TimelineController.TimelineGet), nameof(TimelineController)[0..^nameof(Controller).Length], new { Name = "@" + destination.Username })
            };
            return result;
        }
    }

    public class UserInfoAutoMapperProfile : Profile
    {
        public UserInfoAutoMapperProfile()
        {
            CreateMap<User, UserInfo>().ForMember(u => u._links, opt => opt.MapFrom<UserInfoLinksValueResolver>());
        }
    }
}
