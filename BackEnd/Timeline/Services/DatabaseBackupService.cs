using System.Globalization;
using System.IO;

namespace Timeline.Services
{
    public interface IDatabaseBackupService
    {
        void BackupNow();
    }

    public class DatabaseBackupService : IDatabaseBackupService
    {
        private readonly IPathProvider _pathProvider;
        private readonly IClock _clock;

        public DatabaseBackupService(IPathProvider pathProvider, IClock clock)
        {
            _pathProvider = pathProvider;
            _clock = clock;
        }

        public void BackupNow()
        {
            var databasePath = _pathProvider.GetDatabaseFilePath();
            if (File.Exists(databasePath))
            {
                var backupDirPath = _pathProvider.GetDatabaseBackupDirectory();
                Directory.CreateDirectory(backupDirPath);
                var fileName = _clock.GetCurrentTime().ToString("yyyy-MM-ddTHH-mm-ss", CultureInfo.InvariantCulture);
                var path = Path.Combine(backupDirPath, fileName);
                File.Copy(databasePath, path);
            }
        }
    }
}
