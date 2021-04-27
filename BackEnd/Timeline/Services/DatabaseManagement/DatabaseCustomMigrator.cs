using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Timeline.Entities;

namespace Timeline.Services.DatabaseManagement
{
    public class DatabaseCustomMigrator : IDatabaseCustomMigrator
    {
        private readonly IEnumerable<IDatabaseCustomMigration> _migrations;
        private readonly DatabaseContext _database;

        private readonly ILogger<DatabaseCustomMigrator> _logger;

        public DatabaseCustomMigrator(IEnumerable<IDatabaseCustomMigration> migrations, DatabaseContext database, ILogger<DatabaseCustomMigrator> logger)
        {
            _migrations = migrations;
            _database = database;
            _logger = logger;
        }

        public async Task MigrateAsync(CancellationToken cancellationToken = default)
        {
            foreach (var migration in _migrations)
            {
                var name = migration.GetName();
                var isApplied = await _database.Migrations.AnyAsync(m => m.Name == name, cancellationToken);

                _logger.LogInformation(Resource.DatabaseCustomMigratorFoundMigration, name, isApplied);

                if (!isApplied)
                {
                    _logger.LogWarning(Resource.DatabaseCustomMigratorBeginMigration, name);

                    await using var transaction = await _database.Database.BeginTransactionAsync(cancellationToken);

                    await migration.ExecuteAsync(_database, cancellationToken);

                    _database.Migrations.Add(new MigrationEntity { Name = name });
                    await _database.SaveChangesAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogWarning(Resource.DatabaseCustomMigratorFinishMigration, name);
                }
            }
        }
    }
}
