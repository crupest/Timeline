using Microsoft.Extensions.DependencyInjection;

namespace Timeline.Models.Mapper
{
    public static class MapperServiceCollectionExtensions
    {
        public static void AddMappers(this IServiceCollection services)
        {
            services.AddScoped<UserMapper, UserMapper>();
            services.AddScoped<TimelineMapper, TimelineMapper>();
        }
    }
}
