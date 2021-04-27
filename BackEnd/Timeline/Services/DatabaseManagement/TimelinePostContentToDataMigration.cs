using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Models;
using Timeline.Services.Data;

namespace Timeline.Services.DatabaseManagement
{
    public class TimelinePostContentToDataMigration : IDatabaseCustomMigration
    {
        private readonly IDataManager _dataManager;

        public TimelinePostContentToDataMigration(IDataManager dataManager)
        {
            _dataManager = dataManager;
        }

        public string GetName() => "TimelinePostContentToData";

        public async Task ExecuteAsync(DatabaseContext database, CancellationToken cancellationToken)
        {
#pragma warning disable CS0618
            var postEntities = await database.TimelinePosts.ToListAsync(cancellationToken);

            foreach (var postEntity in postEntities)
            {
                if (postEntity.Content is null)
                {
                    postEntity.Deleted = true;
                }
                else
                {
                    if (postEntity.ContentType == "text")
                    {
                        var tag = await _dataManager.RetainEntryAsync(Encoding.UTF8.GetBytes(postEntity.Content), cancellationToken);
                        database.TimelinePostData.Add(new TimelinePostDataEntity
                        {
                            DataTag = tag,
                            Kind = MimeTypes.TextPlain,
                            Index = 0,
                            PostId = postEntity.Id,
                            LastUpdated = postEntity.LastUpdated
                        });
                    }
                    else
                    {
                        var data = await _dataManager.GetEntryAndCheck(postEntity.Content, Resource.TimelinePostContentToDataMigrationImageNoData, cancellationToken);
                        var format = Image.DetectFormat(data);
                        database.TimelinePostData.Add(new TimelinePostDataEntity
                        {
                            DataTag = postEntity.Content,
                            Kind = format.DefaultMimeType,
                            Index = 0,
                            PostId = postEntity.Id,
                            LastUpdated = postEntity.LastUpdated
                        });
                    }
                }
                postEntity.Content = null;
                postEntity.ContentType = null;
                postEntity.ExtraContent = null;
            }

            await database.SaveChangesAsync(cancellationToken);
#pragma warning restore CS0618
        }
    }
}
