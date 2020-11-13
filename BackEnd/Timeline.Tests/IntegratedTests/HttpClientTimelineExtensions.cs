using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Models.Http;

namespace Timeline.Tests.IntegratedTests
{
    public static class HttpClientTimelineExtensions
    {
        public static async Task<TimelineInfo> GetTimelineAsync(this HttpClient client, string timelineName)
        {
            var res = await client.GetAsync($"timelines/{timelineName}");
            res.Should().HaveStatusCode(HttpStatusCode.OK);
            return await res.Should().HaveAndGetJsonBodyAsync<TimelineInfo>();
        }

        public static async Task<TimelineInfo> PatchTimelineAsync(this HttpClient client, string timelineName, TimelinePatchRequest body)
        {
            var res = await client.PatchAsJsonAsync($"timelines/{timelineName}", body);
            res.Should().HaveStatusCode(HttpStatusCode.OK);
            return await res.Should().HaveAndGetJsonBodyAsync<TimelineInfo>();
        }

        public static async Task PutTimelineMemberAsync(this HttpClient client, string timelineName, string memberUsername)
        {
            var res = await client.PutAsync($"timelines/{timelineName}/members/{memberUsername}");
            res.Should().HaveStatusCode(HttpStatusCode.OK);
        }
    }
}
