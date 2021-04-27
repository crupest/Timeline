using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Timeline.Services.DatabaseManagement
{
    public static class DatabaseManagementServicesServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseManagementService(this IServiceCollection services)
        {
            services.TryAddScoped<IDatabaseCustomMigrator, DatabaseCustomMigrator>();
            services.AddScoped<IDatabaseCustomMigration, TimelinePostContentToDataMigration>();

            services.TryAddScoped<IDatabaseBackupService, DatabaseBackupService>();

            services.AddHostedService<DatabaseManagementService>();
            return services;
        }
    }
}