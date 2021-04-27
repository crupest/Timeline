using System.Threading;
using System.Threading.Tasks;

namespace Timeline.Services.DatabaseManagement
{
    public interface IDatabaseCustomMigrator
    {
        Task MigrateAsync(CancellationToken cancellationToken = default);
    }
}
