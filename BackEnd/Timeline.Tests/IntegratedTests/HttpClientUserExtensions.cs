using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Models.Http;

namespace Timeline.Tests.IntegratedTests
{
    public static class HttpClientUserExtensions
    {
        public static Task<HttpUser> GetUserAsync(this HttpClient client, string username)
            => client.TestGetAsync<HttpUser>($"users/{username}");
    }
}
