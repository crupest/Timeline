using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Timeline.Entities;

namespace Timeline.Services.Data
{
    public class DataManager : IDataManager
    {
        private readonly ILogger<DataManager> _logger;
        private readonly DatabaseContext _database;
        private readonly IETagGenerator _eTagGenerator;

        public DataManager(ILogger<DataManager> logger, DatabaseContext database, IETagGenerator eTagGenerator)
        {
            _logger = logger;
            _database = database;
            _eTagGenerator = eTagGenerator;
        }

        public async Task<string> RetainEntry(byte[] data, CancellationToken cancellationToken = default)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var tag = await _eTagGenerator.GenerateETagAsync(data, cancellationToken);

            var entity = await _database.Data.Where(d => d.Tag == tag).SingleOrDefaultAsync(cancellationToken);
            bool create;


            if (entity == null)
            {
                create = true;
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
                create = false;
                entity.Ref += 1;
            }

            await _database.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(create ? Resource.LogDataManagerRetainEntryCreate : Resource.LogDataManagerRetainEntryAddRefCount, tag);

            return tag;
        }

        public async Task FreeEntry(string tag, CancellationToken cancellationToken = default)
        {
            if (tag == null)
                throw new ArgumentNullException(nameof(tag));

            var entity = await _database.Data.Where(d => d.Tag == tag).SingleOrDefaultAsync(cancellationToken);

            if (entity != null)
            {
                bool remove;

                if (entity.Ref == 1)
                {
                    remove = true;
                    _database.Data.Remove(entity);
                }
                else
                {
                    remove = false;
                    entity.Ref -= 1;
                }

                await _database.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(remove ? Resource.LogDataManagerFreeEntryRemove : Resource.LogDataManagerFreeEntryDecreaseRefCount, tag);
            }
            else
            {
                _logger.LogInformation(Resource.LogDataManagerFreeEntryNotExist, tag);
            }
        }

        public async Task<byte[]?> GetEntry(string tag, CancellationToken cancellationToken = default)
        {
            if (tag == null)
                throw new ArgumentNullException(nameof(tag));

            var entity = await _database.Data.Where(d => d.Tag == tag).Select(d => new { d.Data }).SingleOrDefaultAsync(cancellationToken);

            if (entity is null)
                return null;

            return entity.Data;
        }
    }
}
