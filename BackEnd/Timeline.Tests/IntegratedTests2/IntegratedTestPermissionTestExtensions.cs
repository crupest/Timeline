using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Timeline.Tests.IntegratedTests2
{
    public static class IntegratedTestPermissionTestExtensions
    {
        public static async Task TestOnlySelfAndAdminCanCall(this IntegratedTestBase testBase, HttpMethod httpMethod, string selfResourceUrl, string otherResourceUrl, object? body)
        {
            await testBase.DefaultClient.TestJsonSendAsync(httpMethod, selfResourceUrl, body, expectedStatusCode: HttpStatusCode.Unauthorized);
            await testBase.UserClient.TestJsonSendAsync(httpMethod, selfResourceUrl, body);
            await testBase.UserClient.TestJsonSendAsync(httpMethod, otherResourceUrl, body, expectedStatusCode: HttpStatusCode.Forbidden);
            await testBase.AdminClient.TestJsonSendAsync(httpMethod, selfResourceUrl, body);
        }
    }
}
