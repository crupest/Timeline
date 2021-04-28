using Timeline.Models;

namespace Timeline.Services.Timeline
{
    public class TimelineChangePropertyParams
    {
        public string? Name { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public TimelineVisibility? Visibility { get; set; }
        public string? Color { get; set; }
    }
}
