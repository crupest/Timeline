using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Timeline.Controllers;
using Timeline.Services;

namespace Timeline.Models.Http
{
    public interface IUserInfo
    {
        string Username { get; set; }
        string Nickname { get; set; }
        string AvatarUrl { get; set; }
    }

    public class UserInfo : IUserInfo
    {
        public string Username { get; set; } = default!;
        public string Nickname { get; set; } = default!;
        public string AvatarUrl { get; set; } = default!;
    }

    public class UserInfoForAdmin : IUserInfo
    {
        public string Username { get; set; } = default!;
        public string Nickname { get; set; } = default!;
        public string AvatarUrl { get; set; } = default!;
        public bool Administrator { get; set; }
    }

    public class UserInfoSetAvatarUrlAction : IMappingAction<object, IUserInfo>
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;

        public UserInfoSetAvatarUrlAction(IActionContextAccessor actionContextAccessor, IUrlHelperFactory urlHelperFactory)
        {
            _actionContextAccessor = actionContextAccessor;
            _urlHelperFactory = urlHelperFactory;
        }

        public void Process(object source, IUserInfo destination, ResolutionContext context)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            destination.AvatarUrl = urlHelper.ActionLink(nameof(UserAvatarController.Get), nameof(UserAvatarController), new { destination.Username });
        }
    }

    public class UserInfoAutoMapperProfile : Profile
    {
        public UserInfoAutoMapperProfile()
        {
            CreateMap<User, UserInfo>().AfterMap<UserInfoSetAvatarUrlAction>();
            CreateMap<User, UserInfoForAdmin>().AfterMap<UserInfoSetAvatarUrlAction>();
        }
    }
}
