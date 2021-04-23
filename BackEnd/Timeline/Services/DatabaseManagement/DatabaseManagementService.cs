using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Timeline.Entities;

namespace Timeline.Services.DatabaseManagement
{
    public class DatabaseManagementService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public DatabaseManagementService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var provider = scope.ServiceProvider;

            var backupService = provider.GetRequiredService<IDatabaseBackupService>();
            var database = provider.GetRequiredService<DatabaseContext>();
            var customMigrator = provider.GetRequiredService<IDatabaseCustomMigrator>();


            await backupService.BackupAsync(cancellationToken);
            await database.Database.MigrateAsync(cancellationToken);
            await customMigrator.MigrateAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
