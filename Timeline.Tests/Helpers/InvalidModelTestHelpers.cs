using System.Net.Http;
using System.Threading.Tasks;

namespace Timeline.Tests.Helpers
{
    public static class InvalidModelTestHelpers
    {
        public static async Task TestPostInvalidModel<T>(HttpClient client, string url, T body)
        {
            var response = await client.PostAsJsonAsync(url, body);
            response.Should().HaveStatusCodeBadRequest()
                .And.Should().HaveBodyAsCommonResponseWithCode(ErrorCodes.Http.Common.InvalidModel);
        }

        public static async Task TestPutInvalidModel<T>(HttpClient client, string url, T body)
        {
            var response = await client.PutAsJsonAsync(url, body);
            response.Should().HaveStatusCodeBadRequest()
                .And.Should().HaveBodyAsCommonResponseWithCode(ErrorCodes.Http.Common.InvalidModel);
        }
    }
}
