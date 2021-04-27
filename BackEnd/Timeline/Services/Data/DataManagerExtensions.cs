using System;
using System.Threading;
using System.Threading.Tasks;

namespace Timeline.Services.Data
{
    public static class DataManagerExtensions
    {
        /// <summary>
        /// Try to get an entry and throw <see cref="DatabaseCorruptedException"/> if not exist.
        /// </summary>
        public static async Task<byte[]> GetEntryAndCheck(this IDataManager dataManager, string tag, string notExistMessage, CancellationToken cancellationToken = default)
        {
            if (dataManager is null)
                throw new ArgumentNullException(nameof(dataManager));
            if (tag is null)
                throw new ArgumentNullException(nameof(tag));
            if (notExistMessage is null)
                throw new ArgumentNullException(nameof(notExistMessage));

            var data = await dataManager.GetEntryAsync(tag, cancellationToken);
            if (data is null)
                throw new DatabaseCorruptedException(string.Format(Resource.GetEntryAndCheckNotExist, tag, notExistMessage));
            return data;
        }
    }
}
