using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Models.Http;
using Xunit;

namespace Timeline.Tests.Helpers
{
    public static class InvalidModelTestHelpers
    {
        public static async Task TestPostInvalidModel<T>(HttpClient client, string url, T body)
        {
            var response = await client.PostAsJsonAsync(url, body);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseBody = await response.ReadBodyAsJson<CommonResponse>();
            Assert.Equal(CommonResponse.ErrorCodes.InvalidModel, responseBody.Code);
        }
    }
}
