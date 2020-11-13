using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Timeline.Tests.IntegratedTests
{
    public static class HttpResponseExtensions
    {
        public static async Task<T?> ReadBodyAsJsonAsync<T>(this HttpResponseMessage response)
        {
            return await response.Content.ReadFromJsonAsync<T>(CommonJsonSerializeOptions.Options);
        }
    }
}
