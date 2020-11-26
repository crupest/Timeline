using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using Timeline.Controllers;

namespace Timeline.Models.Http
{
    /// <summary>
    /// Info of post content.
    /// </summary>
    public class HttpTimelinePostContent
    {
        /// <summary>
        /// Type of the post content.
        /// </summary>
        public string Type { get; set; } = default!;
        /// <summary>
        /// If post is of text type. This is the text.
        /// </summary>
        public string? Text { get; set; }
        /// <summary>
        /// If post is of image type. This is the image url.
        /// </summary>
        public string? Url { get; set; }
        /// <summary>
        /// If post has data (currently it means it's a image post), this is the data etag.
        /// </summary>
        public string? ETag { get; set; }
    }

    /// <summary>
    /// Info of a post.
    /// </summary>
    public class HttpTimelinePost
    {
        /// <summary>
        /// Post id.
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// Content of the post. May be null if post is deleted.
        /// </summary>
        public HttpTimelinePostContent? Content { get; set; }
        /// <summary>
        /// True if post is deleted.
        /// </summary>
        public bool Deleted { get; set; }
        /// <summary>
        /// Post time.
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// The author. May be null if the user has been deleted.
        /// </summary>
        public HttpUser? Author { get; set; } = default!;
        /// <summary>
        /// Last updated time.
        /// </summary>
        public DateTime LastUpdated { get; set; } = default!;
    }

    /// <summary>
    /// Info of a timeline.
    /// </summary>
    public class HttpTimeline
    {
        /// <summary>
        /// Unique id.
        /// </summary>
        public string UniqueId { get; set; } = default!;
        /// <summary>
        /// Title.
        /// </summary>
        public string Title { get; set; } = default!;
        /// <summary>
        /// Name of timeline.
        /// </summary>
        public string Name { get; set; } = default!;
        /// <summary>
        /// Last modified time of timeline name.
        /// </summary>
        public DateTime NameLastModifed { get; set; } = default!;
        /// <summary>
        /// Timeline description.
        /// </summary>
        public string Description { get; set; } = default!;
        /// <summary>
        /// Owner of the timeline.
        /// </summary>
        public HttpUser Owner { get; set; } = default!;
        /// <summary>
        /// Visibility of the timeline.
        /// </summary>
        public TimelineVisibility Visibility { get; set; }
#pragma warning disable CA2227 // Collection properties should be read only
        /// <summary>
        /// Members of timeline.
        /// </summary>
        public List<HttpUser> Members { get; set; } = default!;
#pragma warning restore CA2227 // Collection properties should be read only
        /// <summary>
        /// Create time of timeline.
        /// </summary>
        public DateTime CreateTime { get; set; } = default!;
        /// <summary>
        /// Last modified time of timeline.
        /// </summary>
        public DateTime LastModified { get; set; } = default!;

#pragma warning disable CA1707 // Identifiers should not contain underscores
        /// <summary>
        /// Related links.
        /// </summary>
        public HttpTimelineLinks _links { get; set; } = default!;
#pragma warning restore CA1707 // Identifiers should not contain underscores
    }

    /// <summary>
    /// Related links for timeline.
    /// </summary>
    public class HttpTimelineLinks
    {
        /// <summary>
        /// Self.
        /// </summary>
        public string Self { get; set; } = default!;
        /// <summary>
        /// Posts url.
        /// </summary>
        public string Posts { get; set; } = default!;
    }

    public class HttpTimelineLinksValueResolver : IValueResolver<TimelineInfo, HttpTimeline, HttpTimelineLinks>
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;

        public HttpTimelineLinksValueResolver(IActionContextAccessor actionContextAccessor, IUrlHelperFactory urlHelperFactory)
        {
            _actionContextAccessor = actionContextAccessor;
            _urlHelperFactory = urlHelperFactory;
        }

        public HttpTimelineLinks Resolve(TimelineInfo source, HttpTimeline destination, HttpTimelineLinks destMember, ResolutionContext context)
        {
            var actionContext = _actionContextAccessor.AssertActionContextForUrlFill();
            var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);

            return new HttpTimelineLinks
            {
                Self = urlHelper.ActionLink(nameof(TimelineController.TimelineGet), nameof(TimelineController)[0..^nameof(Controller).Length], new { source.Name }),
                Posts = urlHelper.ActionLink(nameof(TimelineController.PostListGet), nameof(TimelineController)[0..^nameof(Controller).Length], new { source.Name })
            };
        }
    }

    public class HttpTimelinePostContentResolver : IValueResolver<TimelinePostInfo, HttpTimelinePost, HttpTimelinePostContent?>
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;

        public HttpTimelinePostContentResolver(IActionContextAccessor actionContextAccessor, IUrlHelperFactory urlHelperFactory)
        {
            _actionContextAccessor = actionContextAccessor;
            _urlHelperFactory = urlHelperFactory;
        }

        public HttpTimelinePostContent? Resolve(TimelinePostInfo source, HttpTimelinePost destination, HttpTimelinePostContent? destMember, ResolutionContext context)
        {
            var actionContext = _actionContextAccessor.AssertActionContextForUrlFill();
            var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);

            var sourceContent = source.Content;

            if (sourceContent == null)
            {
                return null;
            }

            if (sourceContent is TextTimelinePostContent textContent)
            {
                return new HttpTimelinePostContent
                {
                    Type = TimelinePostContentTypes.Text,
                    Text = textContent.Text
                };
            }
            else if (sourceContent is ImageTimelinePostContent imageContent)
            {
                return new HttpTimelinePostContent
                {
                    Type = TimelinePostContentTypes.Image,
                    Url = urlHelper.ActionLink(
                        action: nameof(TimelineController.PostDataGet),
                        controller: nameof(TimelineController)[0..^nameof(Controller).Length],
                        values: new { Name = source.TimelineName, Id = source.Id }),
                    ETag = $"\"{imageContent.DataTag}\""
                };
            }
            else
            {
                throw new InvalidOperationException(Resources.Models.Http.Exception.UnknownPostContentType);
            }
        }
    }

    public class HttpTimelineAutoMapperProfile : Profile
    {
        public HttpTimelineAutoMapperProfile()
        {
            CreateMap<TimelineInfo, HttpTimeline>().ForMember(u => u._links, opt => opt.MapFrom<HttpTimelineLinksValueResolver>());
            CreateMap<TimelinePostInfo, HttpTimelinePost>().ForMember(p => p.Content, opt => opt.MapFrom<HttpTimelinePostContentResolver>());
        }
    }
}
