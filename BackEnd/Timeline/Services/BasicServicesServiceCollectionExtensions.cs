using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
