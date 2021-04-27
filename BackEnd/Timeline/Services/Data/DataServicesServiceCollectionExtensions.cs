using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Timeline.Services.Data
{
    public static class DataServicesServiceCollectionExtensions
    {
        public static IServiceCollection AddDataServices(this IServiceCollection services)
        {
            services.TryAddScoped<IETagGenerator, ETagGenerator>();
            services.TryAddScoped<IDataManager, DataManager>();
            return services;
        }
    }
}
