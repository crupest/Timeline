using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Timeline.Services.Timeline
{
    public static class TimelineServicesServiceCollectionExtensions
    {
        public static IServiceCollection AddTimelineServices(this IServiceCollection services)
        {
            services.TryAddScoped<IBasicTimelineService, BasicTimelineService>();
            services.TryAddScoped<ITimelineService, TimelineService>();
            services.TryAddScoped<ITimelinePostService, TimelinePostService>();
            services.TryAddScoped<MarkdownProcessor>();
            return services;
        }
    }
}
