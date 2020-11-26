using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Models.Http;

namespace Timeline.Tests.IntegratedTests
{
    public static class HttpClientTimelineExtensions
    {
        public static Task<HttpTimeline> GetTimelineAsync(this HttpClient client, string timelineName)
            => client.TestGetAsync<HttpTimeline>($"timelines/{timelineName}");

        public static Task<HttpTimeline> PatchTimelineAsync(this HttpClient client, string timelineName, HttpTimelinePatchRequest body)
            => client.TestPatchAsync<HttpTimeline>($"timelines/{timelineName}", body);

        public static Task PutTimelineMemberAsync(this HttpClient client, string timelineName, string memberUsername)
            => client.TestPutAsync($"timelines/{timelineName}/members/{memberUsername}");

        public static Task DeleteTimelineMemberAsync(this HttpClient client, string timelineName, string memberUsername, bool? delete)
            => client.TestDeleteAsync($"timelines/{timelineName}/members/{memberUsername}", delete);
    }
}
