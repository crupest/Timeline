using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Models.Http;

namespace Timeline.Tests.IntegratedTests
{
    public static class HttpClientUserExtensions
    {
        public static async Task<UserInfo> GetUserAsync(this HttpClient client, string username)
        {
            var res = await client.GetAsync($"users/{username}");
            res.Should().HaveStatusCode(HttpStatusCode.OK);
            return await res.Should().HaveAndGetJsonBodyAsync<UserInfo>();
        }
    }
}
