using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Timeline.Services
{
    public class DataEntry
    {
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] Data { get; set; } = default!;
#pragma warning restore CA1819 // Properties should not return arrays
        public DataInfo Info { get; set; } = default!;
    }

    public class DataInfo
    {
        public DateTime Time { get; set; }
        public string Type { get; set; } = default!;
    }

    /// <summary>
    /// A data manager controling data.
    /// </summary>
    /// <remarks>
    /// All data to be saved will be checked identity.
    /// Identical data will be saved as one copy and return the same tag.
    /// Every data has a ref count. When data is saved, ref count increase.
    /// When data is removed, ref count decease. If ref count is decreased
    /// to 0, the data entry will be destroyed and no longer occupy space.
    /// 
    /// Type is just an attached attribute for convenience and not participate
    /// in identity verification. This should be only used to save blobs but not
    /// strings. It will be rare for identity blob with different type, I think.
    /// </remarks>
    public interface IDataManager
    {
        /// <summary>
        /// Saves the data to a new entry if it does not exist, 
        /// increases its ref count and returns a tag to the entry.
        /// </summary>
        /// <param name="data">The data. Can't be null.</param>
        /// <param name="type">The type of the data. Can't be null.</param>
        /// <returns>The tag of the created entry.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> or <paramref name="type"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when a saved copy of data already exists but type is different.</exception>
        public Task<string> RetainEntry(byte[] data, string type);

        /// <summary>
        /// Decrease the the ref count of the entry.
        /// Remove it if ref count is zero.
        /// </summary>
        /// <param name="tag">The tag of the entry.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tag"/> is null.</exception>
        /// <remarks>
        /// It's no-op if entry with tag does not exist.
        /// </remarks>
        public Task FreeEntry(string tag);

        /// <summary>
        /// Retrieve the entry with given tag.
        /// </summary>
        /// <param name="tag">The tag of the entry.</param>
        /// <returns>The entry.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tag"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when entry with given tag does not exist.</exception>
        public Task<DataEntry> GetEntry(string tag);

        /// <summary>
        /// Retrieve info of the entry with given tag.
        /// </summary>
        /// <param name="tag">The tag of the entry.</param>
        /// <returns>The entry info.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tag"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when entry with given tag does not exist.</exception>
        public Task<DataInfo> GetEntryInfo(string tag);
    }

}
