using System.Collections.Generic;
using System.Threading.Tasks;

namespace Timeline.Services.Timeline
{
    public static class BasicTimelineServiceExtensions
    {
        public static async Task ThrowIfTimelineNotExist(this IBasicTimelineService service, long timelineId)
        {
            if (!await service.CheckTimelineExistenceAsync(timelineId))
            {
                throw new EntityNotExistException(EntityTypes.Timeline,
                    new Dictionary<string, object> { ["id"] = timelineId });
            }
        }
    }
}
