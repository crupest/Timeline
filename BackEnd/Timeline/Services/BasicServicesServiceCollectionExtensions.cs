using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Timeline.Services
{
    public static class BasicServicesServiceCollectionExtensions
    {
        public static IServiceCollection AddBasicServices(this IServiceCollection services)
        {
            services.TryAddSingleton<IPathProvider, PathProvider>();
            services.TryAddTransient<IClock, Clock>();
            return services;
        }
    }
}
