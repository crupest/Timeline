using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Resources;
using Timeline.Entities;
using Timeline.Services;

[assembly: NeutralResourcesLanguage("en")]

namespace Timeline
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            var env = host.Services.GetRequiredService<IWebHostEnvironment>();

            var databaseBackupService = host.Services.GetRequiredService<IDatabaseBackupService>();
            databaseBackupService.BackupNow();

            if (env.IsProduction())
            {
                using (var scope = host.Services.CreateScope())
                {
                    var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                    databaseContext.Database.Migrate();
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
