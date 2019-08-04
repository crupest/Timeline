using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace Timeline.Tests.Helpers
{
    public static class ResponseExtensions
    {
        public static async Task<T> ReadBodyAsJson<T>(this HttpResponseMessage response)
        {
            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }
    }
}
