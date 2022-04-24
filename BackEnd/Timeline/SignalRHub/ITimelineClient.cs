using System;
using System.Threading.Tasks;

namespace Timeline.SignalRHub
{
    public interface ITimelineClient
    {
        [Obsolete("Use v2.")]
        Task OnTimelinePostChanged(string timelineName);
        Task OnTimelinePostChangedV2(string owner, string timeline);
    }
}
