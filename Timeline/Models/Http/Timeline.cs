using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using TimelineApp.Controllers;

namespace TimelineApp.Models.Http
{
    public class TimelinePostContentInfo
    {
        public string Type { get; set; } = default!;
        public string? Text { get; set; }
        public string? Url { get; set; }
    }

    public class TimelinePostInfo
    {
        public long Id { get; set; }
        public TimelinePostContentInfo Content { get; set; } = default!;
        public DateTime Time { get; set; }
        public UserInfo Author { get; set; } = default!;
        public DateTime LastUpdated { get; set; } = default!;
    }

    public class TimelineInfo
    {
        public string? Name { get; set; }
        public string Description { get; set; } = default!;
        public UserInfo Owner { get; set; } = default!;
        public TimelineVisibility Visibility { get; set; }
#pragma warning disable CA2227 // Collection properties should be read only
        public List<UserInfo> Members { get; set; } = default!;
#pragma warning restore CA2227 // Collection properties should be read only

#pragma warning disable CA1707 // Identifiers should not contain underscores
        public TimelineInfoLinks _links { get; set; } = default!;
#pragma warning restore CA1707 // Identifiers should not contain underscores
    }

    public class TimelineInfoLinks
    {
        public string Self { get; set; } = default!;
        public string Posts { get; set; } = default!;
    }

    public class TimelineInfoLinksValueResolver : IValueResolver<Timeline, TimelineInfo, TimelineInfoLinks>
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;

        public TimelineInfoLinksValueResolver(IActionContextAccessor actionContextAccessor, IUrlHelperFactory urlHelperFactory)
        {
            _actionContextAccessor = actionContextAccessor;
            _urlHelperFactory = urlHelperFactory;
        }

        public TimelineInfoLinks Resolve(Timeline source, TimelineInfo destination, TimelineInfoLinks destMember, ResolutionContext context)
        {
            if (_actionContextAccessor.ActionContext == null)
                throw new InvalidOperationException("No action context, can't fill urls.");

            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);


            return new TimelineInfoLinks
            {
                Self = urlHelper.ActionLink(nameof(TimelineController.TimelineGet), nameof(TimelineController)[0..^nameof(Controller).Length], new { source.Name }),
                Posts = urlHelper.ActionLink(nameof(TimelineController.PostListGet), nameof(TimelineController)[0..^nameof(Controller).Length], new { source.Name })
            };
        }
    }

    public class TimelinePostConverter : ITypeConverter<ITimelinePostContent, TimelinePostContentInfo>
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;

        public TimelinePostConverter(IActionContextAccessor actionContextAccessor, IUrlHelperFactory urlHelperFactory)
        {
            _actionContextAccessor = actionContextAccessor;
            _urlHelperFactory = urlHelperFactory;
        }

        public TimelinePostContentInfo Convert(ITimelinePostContent source, TimelinePostContentInfo destination, ResolutionContext context)
        {
            if (_actionContextAccessor.ActionContext == null)
                throw new InvalidOperationException("No action context, can't fill urls.");

            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            if (source is TextTimelinePostContent textContent)
            {
                return new TimelinePostContentInfo
                {
                    Type = TimelinePostContentTypes.Text,
                    Text = textContent.Text
                };
            }
            else if (source is ImageTimelinePostContent imageContent)
            {
                return new TimelinePostContentInfo
                {
                    Type = TimelinePostContentTypes.Image,
                    Url = urlHelper.ActionLink(action: "PostDataGet", nameof(TimelineController)[0..^nameof(Controller).Length], new { source.Name })
                };
            }
        }
    }

    public class TimelineInfoAutoMapperProfile : Profile
    {
        public TimelineInfoAutoMapperProfile()
        {
            CreateMap<Timeline, TimelineInfo>().ForMember(u => u._links, opt => opt.MapFrom<TimelineInfoLinksValueResolver>());
            CreateMap<TimelinePost, TimelinePostInfo>();
            CreateMap<ITimelinePostContent, TimelinePostContentInfo>().ConvertUsing<TimelinePostConverter>();
            CreateMap<TimelinePatchRequest, TimelineChangePropertyRequest>();
        }
    }
}
