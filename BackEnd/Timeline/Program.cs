using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Resources;
using Timeline.Entities;
using Timeline.Services;
using Timeline.Services.Migration;

[assembly: NeutralResourcesLanguage("en")]

namespace Timeline
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var databaseBackupService = scope.ServiceProvider.GetRequiredService<IDatabaseBackupService>();
                databaseBackupService.BackupNow();

                var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                databaseContext.Database.Migrate();

                var customMigrationManager = scope.ServiceProvider.GetRequiredService<ICustomMigrationManager>();
                customMigrationManager.Migrate();
            }

            host.Run();
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(config =>
                {
                    config.AddEnvironmentVariables("Timeline_");
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
