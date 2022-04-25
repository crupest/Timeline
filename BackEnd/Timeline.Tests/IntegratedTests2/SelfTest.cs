using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Models.Http;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests.IntegratedTests2
{
    public class SelfTest : IntegratedTestBase
    {
        public SelfTest(ITestOutputHelper testOutput) : base(testOutput)
        {
        }

        [Fact]
        public async Task ChangePassword()
        {
            await DefaultClient.TestJsonSendAsync(HttpMethod.Post, "v2/self/changepassword", new HttpChangePasswordRequest
            {
                OldPassword = "abc",
                NewPassword = "def"
            }, expectedStatusCode: HttpStatusCode.Unauthorized);


            await UserClient.TestJsonSendAsync(HttpMethod.Post, "v2/self/changepassword", new HttpChangePasswordRequest
            {
                OldPassword = "abc",
                NewPassword = "def"
            }, expectedStatusCode: HttpStatusCode.UnprocessableEntity);

            await UserClient.TestJsonSendAsync(HttpMethod.Post, "v2/self/changepassword", new HttpChangePasswordRequest
            {
                OldPassword = "userpw",
                NewPassword = "def"
            }, expectedStatusCode: HttpStatusCode.NoContent);
        }
    }
}

