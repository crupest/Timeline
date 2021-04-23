using System.Threading;
using System.Threading.Tasks;
using Timeline.Entities;

namespace Timeline.Services.DatabaseManagement
{
    public interface IDatabaseCustomMigration
    {
        string GetName();
        Task ExecuteAsync(DatabaseContext database, CancellationToken cancellationToken = default);
    }
}
