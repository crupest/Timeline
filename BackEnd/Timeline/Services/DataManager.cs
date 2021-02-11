using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;

namespace Timeline.Services
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
        /// <param name="saveDatabaseChange">If true save database change. Otherwise it does not save database change.</param>
        /// <returns>The tag of the created entry.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
        public Task<string> RetainEntry(byte[] data, bool saveDatabaseChange = true);

        /// <summary>
        /// Decrease the the ref count of the entry.
        /// Remove it if ref count is zero.
        /// </summary>
        /// <param name="tag">The tag of the entry.</param>
        /// <param name="saveDatabaseChange">If true save database change. Otherwise it does not save database change.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tag"/> is null.</exception>
        /// <remarks>
        /// It's no-op if entry with tag does not exist.
        /// </remarks>
        public Task FreeEntry(string tag, bool saveDatabaseChange = true);

        /// <summary>
        /// Retrieve the entry with given tag. If not exist, returns null.
        /// </summary>
        /// <param name="tag">The tag of the entry.</param>
        /// <returns>The data of the entry. If not exist, returns null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tag"/> is null.</exception>
        public Task<byte[]?> GetEntry(string tag);
    }

    public class DataManager : IDataManager
    {
        private readonly DatabaseContext _database;
        private readonly IETagGenerator _eTagGenerator;

        public DataManager(DatabaseContext database, IETagGenerator eTagGenerator)
        {
            _database = database;
            _eTagGenerator = eTagGenerator;
        }

        public async Task<string> RetainEntry(byte[] data, bool saveDatabaseChange = true)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var tag = await _eTagGenerator.Generate(data);

            var entity = await _database.Data.Where(d => d.Tag == tag).SingleOrDefaultAsync();

            if (entity == null)
            {
                entity = new DataEntity
                {
                    Tag = tag,
                    Data = data,
                    Ref = 1
                };
                _database.Data.Add(entity);
            }
            else
            {
                entity.Ref += 1;
            }

            if (saveDatabaseChange)
                await _database.SaveChangesAsync();

            return tag;
        }

        public async Task FreeEntry(string tag, bool saveDatabaseChange)
        {
            if (tag == null)
                throw new ArgumentNullException(nameof(tag));

            var entity = await _database.Data.Where(d => d.Tag == tag).SingleOrDefaultAsync();

            if (entity != null)
            {
                if (entity.Ref == 1)
                {
                    _database.Data.Remove(entity);
                }
                else
                {
                    entity.Ref -= 1;
                }

                if (saveDatabaseChange)
                    await _database.SaveChangesAsync();
            }
        }

        public async Task<byte[]?> GetEntry(string tag)
        {
            if (tag == null)
                throw new ArgumentNullException(nameof(tag));

            var entity = await _database.Data.Where(d => d.Tag == tag).Select(d => new { d.Data }).SingleOrDefaultAsync();

            if (entity is null)
                return null;

            return entity.Data;
        }
    }

    public static class DataManagerExtensions
    {
        /// <summary>
        /// Try to get an entry and throw <see cref="DatabaseCorruptedException"/> if not exist.
        /// </summary>
        public static async Task<byte[]> GetEntryAndCheck(this IDataManager dataManager, string tag, string notExistMessage)
        {
            var data = await dataManager.GetEntry(tag);
            if (data is null)
                throw new DatabaseCorruptedException($"Can't get data of tag {tag}. {notExistMessage}");
            return data;
        }
    }
}
