using System;
using System.Threading;
using System.Threading.Tasks;

namespace Timeline.Services.Data
{
    /// <summary>
    /// A data manager controlling data.
    /// </summary>
    /// <remarks>
    /// Identical data will be saved as one copy and return the same tag.
    /// Every data has a ref count. When data is retained, ref count increase.
    /// When data is freed, ref count decease. If ref count is decreased
    /// to 0, the data entry will be destroyed and no longer occupy space.
    /// </remarks>
    public interface IDataManager
    {
        /// <summary>
        /// Saves the data to a new entry if it does not exist, 
        /// increases its ref count and returns a tag to the entry.
        /// </summary>
        /// <param name="data">The data. Can't be null.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The tag of the created entry.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
        public Task<string> RetainEntry(byte[] data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Decrease the the ref count of the entry.
        /// Remove it if ref count is zero.
        /// </summary>
        /// <param name="tag">The tag of the entry.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tag"/> is null.</exception>
        /// <remarks>
        /// It's no-op if entry with tag does not exist.
        /// </remarks>
        public Task FreeEntry(string tag, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieve the entry with given tag. If not exist, returns null.
        /// </summary>
        /// <param name="tag">The tag of the entry.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The data of the entry. If not exist, returns null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tag"/> is null.</exception>
        public Task<byte[]?> GetEntry(string tag, CancellationToken cancellationToken = default);
    }
}
