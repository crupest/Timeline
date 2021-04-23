using Microsoft.Extensions.DependencyInjection;

namespace Timeline.Services.DatabaseManagement
{
    public static class DatabaseManagementServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseManagementService(this IServiceCollection services)
        {
            services.AddScoped<IDatabaseCustomMigrator, DatabaseCustomMigrator>();
            services.AddScoped<IDatabaseCustomMigration, TimelinePostContentToDataMigration>();
            services.AddScoped<IDatabaseBackupService, DatabaseBackupService>();
            services.AddHostedService<DatabaseManagementService>();
            return services;
        }
    }
}