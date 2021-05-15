using System.Threading.Tasks;

namespace Timeline.SignalRHub
{
    public interface ITimelineClient
    {
        Task OnTimelinePostChanged(string timelineName);
    }
}
