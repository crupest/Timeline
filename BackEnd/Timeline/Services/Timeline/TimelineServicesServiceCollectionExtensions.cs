﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Timeline.Services.Timeline
{
    public static class TimelineServicesServiceCollectionExtensions
    {
        public static IServiceCollection AddTimelineServices(this IServiceCollection services)
        {
            services.TryAddScoped<ITimelineService, TimelineService>();
            services.TryAddScoped<ITimelinePostService, TimelinePostService>();
            services.TryAddScoped<MarkdownProcessor>();
            return services;
        }
    }
}
