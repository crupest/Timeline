using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Timeline.Entities;

namespace Timeline.Services.Migration
{
    public interface ICustomMigrationManager
    {
        Task Migrate();
    }

    public class CustomMigrationManager : ICustomMigrationManager
    {
        private IEnumerable<ICustomMigration> _migrations;
        private DatabaseContext _database;

        private ILogger<CustomMigrationManager> _logger;

        public CustomMigrationManager(IEnumerable<ICustomMigration> migrations, DatabaseContext database, ILogger<CustomMigrationManager> logger)
        {
            _migrations = migrations;
            _database = database;
            _logger = logger;
        }

        public async Task Migrate()
        {
            foreach (var migration in _migrations)
            {
                var name = migration.GetName();
                var did = await _database.Migrations.AnyAsync(m => m.Name == name);

                _logger.LogInformation("Found custom migration '{0}'. Did: {1}.", name, did);

                if (!did)
                {
                    _logger.LogInformation("Begin custom migration '{0}'.", name);

                    await using var transaction = await _database.Database.BeginTransactionAsync();

                    await migration.Execute(_database);

                    _database.Migrations.Add(new MigrationEntity { Name = name });
                    await _database.SaveChangesAsync();

                    await transaction.CommitAsync();

                    _logger.LogInformation("End custom migration '{0}'.", name);
                }
            }
        }
    }
}
