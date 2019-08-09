using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Models.Http;
using Xunit;

namespace Timeline.Tests.Helpers
{
    public static class ResponseExtensions
    {
        public static void AssertOk(this HttpResponseMessage response)
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        public static void AssertNotFound(this HttpResponseMessage response)
        {
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        public static void AssertBadRequest(this HttpResponseMessage response)
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        public static async Task AssertIsPutCreated(this HttpResponseMessage response)
        {
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.ReadBodyAsJson<CommonResponse>();
            Assert.Equal(CommonPutResponse.CreatedCode, body.Code);
        }

        public static async Task AssertIsPutModified(this HttpResponseMessage response)
        {
            response.AssertOk();
            var body = await response.ReadBodyAsJson<CommonResponse>();
            Assert.Equal(CommonPutResponse.ModifiedCode, body.Code);
        }


        public static async Task AssertIsDeleteDeleted(this HttpResponseMessage response)
        {
            response.AssertOk();
            var body = await response.ReadBodyAsJson<CommonResponse>();
            Assert.Equal(CommonDeleteResponse.DeletedCode, body.Code);
        }

        public static async Task AssertIsDeleteNotExist(this HttpResponseMessage response)
        {
            response.AssertOk();
            var body = await response.ReadBodyAsJson<CommonResponse>();
            Assert.Equal(CommonDeleteResponse.NotExistsCode, body.Code);
        }

        public static async Task<T> ReadBodyAsJson<T>(this HttpResponseMessage response)
        {
            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }
    }
}
