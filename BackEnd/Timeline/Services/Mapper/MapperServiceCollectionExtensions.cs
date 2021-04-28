using Microsoft.Extensions.DependencyInjection;
using Timeline.Entities;
using Timeline.Models.Http;

namespace Timeline.Services.Mapper
{
    public static class MapperServiceCollectionExtensions
    {
        public static void AddMappers(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Startup).Assembly);
            services.AddScoped<IMapper<UserEntity, HttpUser>, UserMapper>();
            services.AddScoped<IMapper<TimelineEntity, HttpTimeline>, TimelineMapper>();
            services.AddScoped<IMapper<TimelinePostEntity, HttpTimelinePost>, TimelineMapper>();
            services.AddScoped<IGenericMapper, GenericMapper>();
        }
    }
}
