using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Timeline.Entities;

namespace Timeline.Services.DatabaseManagement
{
    public interface IDatabaseBackupService
    {
        Task BackupAsync(CancellationToken cancellationToken = default);
    }

    public class DatabaseBackupService : IDatabaseBackupService
    {
        private readonly DatabaseContext _database;
        private readonly IPathProvider _pathProvider;
        private readonly IClock _clock;

        public DatabaseBackupService(DatabaseContext database, IPathProvider pathProvider, IClock clock)
        {
            _database = database;
            _pathProvider = pathProvider;
            _clock = clock;
        }

        public async Task BackupAsync(CancellationToken cancellationToken = default)
        {
            var backupDirPath = _pathProvider.GetDatabaseBackupDirectory();
            Directory.CreateDirectory(backupDirPath);
            var fileName = _clock.GetCurrentTime().ToString("yyyy-MM-ddTHH-mm-ss", CultureInfo.InvariantCulture);
            var path = Path.Combine(backupDirPath, fileName);
            await _database.Database.ExecuteSqlInterpolatedAsync($"VACUUM INTO {path}", cancellationToken);
        }
    }
}
