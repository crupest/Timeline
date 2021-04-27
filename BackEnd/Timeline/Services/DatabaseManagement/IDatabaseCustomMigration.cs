using System.Threading;
using System.Threading.Tasks;
using Timeline.Entities;

namespace Timeline.Services.DatabaseManagement
{
    public interface IDatabaseCustomMigration
    {
        string GetName();

        /// <summary>
        /// Execute the migration on database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <remarks>
        /// Do not create transaction since the migrator will take care of transaction.
        /// </remarks>
        Task ExecuteAsync(DatabaseContext database, CancellationToken cancellationToken = default);
    }
}
