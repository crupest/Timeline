using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Models.Http;

namespace Timeline.Tests.IntegratedTests
{
    public static class HttpClientTimelineExtensions
    {
        public static Task<TimelineInfo> GetTimelineAsync(this HttpClient client, string timelineName)
            => client.TestGetAsync<TimelineInfo>($"timelines/{timelineName}");

        public static Task<TimelineInfo> PatchTimelineAsync(this HttpClient client, string timelineName, TimelinePatchRequest body)
            => client.TestPatchAsync<TimelineInfo>($"timelines/{timelineName}", body);

        public static Task PutTimelineMemberAsync(this HttpClient client, string timelineName, string memberUsername)
            => client.TestPutAsync($"timelines/{timelineName}/members/{memberUsername}");

        public static Task DeleteTimelineMemberAsync(this HttpClient client, string timelineName, string memberUsername, bool? delete)
            => client.TestDeleteAsync($"timelines/{timelineName}/members/{memberUsername}", delete);
    }
}
