using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Timeline.Entities;

namespace Timeline.Services.DatabaseManagement
{
    public class DatabaseManagementService : IHostedService
    {
        private readonly DatabaseContext _database;
        private readonly IDatabaseBackupService _backupService;
        private readonly IDatabaseCustomMigrator _customMigrator;

        public DatabaseManagementService(DatabaseContext database, IDatabaseBackupService backupService, IDatabaseCustomMigrator customMigrator)
        {
            _database = database;
            _backupService = backupService;
            _customMigrator = customMigrator;
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            await _backupService.BackupAsync(cancellationToken);
            await _database.Database.MigrateAsync(cancellationToken);
            await _customMigrator.MigrateAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
