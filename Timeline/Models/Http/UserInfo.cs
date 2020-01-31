using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Timeline.Controllers;
using Timeline.Services;

namespace Timeline.Models.Http
{
    public class UserInfo
    {
        public string Username { get; set; } = default!;
        public string Nickname { get; set; } = default!;
        public bool? Administrator { get; set; } = default!;
#pragma warning disable CA1707 
        public UserInfoLinks? _links { get; set; }
#pragma warning restore CA1707
    }

    public class UserInfoLinks
    {
        public string Avatar { get; set; } = default!;
        public string Timeline { get; set; } = default!;
    }

    public class UserInfoLinksValueResolver : IValueResolver<User, UserInfo, UserInfoLinks?>
    {
        private readonly IActionContextAccessor? _actionContextAccessor;
        private readonly IUrlHelperFactory? _urlHelperFactory;

        public UserInfoLinksValueResolver()
        {
            _actionContextAccessor = null;
            _urlHelperFactory = null;
        }

        public UserInfoLinksValueResolver(IActionContextAccessor actionContextAccessor, IUrlHelperFactory urlHelperFactory)
        {
            _actionContextAccessor = actionContextAccessor;
            _urlHelperFactory = urlHelperFactory;
        }

        public UserInfoLinks? Resolve(User source, UserInfo destination, UserInfoLinks? destMember, ResolutionContext context)
        {
            if (_actionContextAccessor == null || _urlHelperFactory == null)
            {
                return null;
            }

            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            return new UserInfoLinks
            {
                Avatar = urlHelper.ActionLink(nameof(UserAvatarController.Get), nameof(UserAvatarController), new { destination.Username }),
                Timeline = urlHelper.ActionLink(nameof(PersonalTimelineController.TimelineGet), nameof(PersonalTimelineController), new { destination.Username })
            };
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
