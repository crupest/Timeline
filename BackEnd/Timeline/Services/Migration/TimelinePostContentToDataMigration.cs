using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using Timeline.Entities;
using Timeline.Models;

namespace Timeline.Services.Migration
{
    public class TimelinePostContentToDataMigration : ICustomMigration
    {
        private readonly IDataManager _dataManager;

        public TimelinePostContentToDataMigration(IDataManager dataManager)
        {
            _dataManager = dataManager;
        }

        public string GetName() => "TimelinePostContentToData";

        public async Task Execute(DatabaseContext database)
        {
#pragma warning disable CS0618
            var postEntities = await database.TimelinePosts.ToListAsync();

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
                        var tag = await _dataManager.RetainEntry(Encoding.UTF8.GetBytes(postEntity.Content), false);
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
                        var data = await _dataManager.GetEntryAndCheck(postEntity.Content, "Old image content does not have corresponding data with the tag.");
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

            await database.SaveChangesAsync();
#pragma warning restore CS0618
        }
    }
}
