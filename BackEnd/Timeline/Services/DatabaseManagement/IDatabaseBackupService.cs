using System.Threading;
using System.Threading.Tasks;

namespace Timeline.Services.DatabaseManagement
{
    public interface IDatabaseBackupService
    {
        Task BackupAsync(CancellationToken cancellationToken = default);
    }
}
