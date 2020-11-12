using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Collections.Generic;
using Timeline.Controllers;
using Timeline.Services;

namespace Timeline.Models.Http
{
    /// <summary>
    /// Info of a user.
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// Unique id.
        /// </summary>
        public string UniqueId { get; set; } = default!;
        /// <summary>
        /// Username.
        /// </summary>
        public string Username { get; set; } = default!;
        /// <summary>
        /// Nickname.
        /// </summary>
        public string Nickname { get; set; } = default!;
        /// <summary>
        /// True if the user is a administrator.
        /// </summary>
        public bool? Administrator { get; set; } = default!;
#pragma warning disable CA2227 // Collection properties should be read only
        /// <summary>
        /// The permissions of the user.
        /// </summary>
        public List<string> Permissions { get; set; } = default!;
#pragma warning restore CA2227 // Collection properties should be read only
#pragma warning disable CA1707 // Identifiers should not contain underscores
        /// <summary>
        /// Related links.
        /// </summary>
        public UserInfoLinks _links { get; set; } = default!;
#pragma warning restore CA1707 // Identifiers should not contain underscores
    }

    /// <summary>
    /// Related links for user.
    /// </summary>
    public class UserInfoLinks
    {
        /// <summary>
        /// Self.
        /// </summary>
        public string Self { get; set; } = default!;
        /// <summary>
        /// Avatar url.
        /// </summary>
        public string Avatar { get; set; } = default!;
        /// <summary>
        /// Personal timeline url.
        /// </summary>
        public string Timeline { get; set; } = default!;
    }

    public class UserPermissionsValueConverter : ITypeConverter<UserPermissions, List<string>>
    {
        public List<string> Convert(UserPermissions source, List<string> destination, ResolutionContext context)
        {
            return source.ToStringList();
        }
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
            var actionContext = _actionContextAccessor.AssertActionContextForUrlFill();
            var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);

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
            CreateMap<UserPermissions, List<string>>()
                .ConvertUsing<UserPermissionsValueConverter>();
            CreateMap<User, UserInfo>()
                .ForMember(u => u._links, opt => opt.MapFrom<UserInfoLinksValueResolver>());
        }
    }
}
