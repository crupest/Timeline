using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Timeline.Entities;

namespace Timeline.Services.DatabaseManagement
{
    public interface IDatabaseCustomMigrator
    {
        Task MigrateAsync(CancellationToken cancellationToken = default);
    }

    public class DatabaseCustomMigrator : IDatabaseCustomMigrator
    {
        private IEnumerable<IDatabaseCustomMigration> _migrations;
        private DatabaseContext _database;

        private ILogger<DatabaseCustomMigrator> _logger;

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

                _logger.LogInformation("Found custom migration '{0}'. Applied: {1}.", name, isApplied);

                if (!isApplied)
                {
                    _logger.LogWarning("Begin custom migration '{0}'.", name);

                    await using var transaction = await _database.Database.BeginTransactionAsync(cancellationToken);

                    await migration.ExecuteAsync(_database, cancellationToken);

                    _database.Migrations.Add(new MigrationEntity { Name = name });
                    await _database.SaveChangesAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogWarning("End custom migration '{0}'.", name);
                }
            }
        }
    }
}
