using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Timeline.Services.Imaging
{
    public static class ImageServicesServiceCollectionExtensions
    {
        public static IServiceCollection AddImageServices(this IServiceCollection services)
        {
            services.TryAddTransient<IImageService, ImageService>();
            return services;
        }
    }
}
