using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Resources;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Services;
using Timeline.Services.Migration;

[assembly: NeutralResourcesLanguage("en")]

namespace Timeline
{
    public static class Program
    {
        public async static Task Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            var databaseBackupService = host.Services.GetRequiredService<IDatabaseBackupService>();
            databaseBackupService.BackupNow();

            using (var scope = host.Services.CreateScope())
            {
                var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                await databaseContext.Database.MigrateAsync();
                var customMigrationManager = scope.ServiceProvider.GetRequiredService<ICustomMigrationManager>();
                await customMigrationManager.Migrate();
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
