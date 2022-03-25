using System.Collections.Generic;
using System.Threading.Tasks;
using Timeline.Entities;

namespace Timeline.Services.Timeline
{
    public static class TimelineServiceExtensions
    {
        public static async Task ThrowIfTimelineNotExist(this ITimelineService service, long timelineId)
        {
            if (!await service.CheckTimelineExistenceAsync(timelineId))
            {
                throw new EntityNotExistException(EntityTypes.Timeline,
                    new Dictionary<string, object> { ["id"] = timelineId });
            }
        }

        public static async Task<List<TimelineEntity>> GetTimelineList(this ITimelineService service, IEnumerable<long> ids)
        {
            var timelines = new List<TimelineEntity>();
            foreach (var id in ids)
            {
                timelines.Add(await service.GetTimelineAsync(id));
            }
            return timelines;
        }
    }
}
