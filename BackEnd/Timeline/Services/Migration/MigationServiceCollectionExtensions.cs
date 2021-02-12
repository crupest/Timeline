using Microsoft.Extensions.DependencyInjection;

namespace Timeline.Services.Migration
{
    public static class MigrationServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomMigration(this IServiceCollection services)
        {
            services.AddScoped<ICustomMigrationManager, CustomMigrationManager>();
            services.AddScoped<ICustomMigration, TimelinePostContentToDataMigration>();
            return services;
        }
    }
}