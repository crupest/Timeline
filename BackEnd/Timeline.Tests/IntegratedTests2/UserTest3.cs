using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Timeline.Models.Http;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests.IntegratedTests2
{
    public class UserTest3 : IntegratedTestBase
    {
        public UserTest3(ITestOutputHelper testOutput) : base(testOutput)
        {
        }

        [Fact]
        public async Task UserPermissionTest()
        {
            await AdminClient.TestSendAsync(HttpMethod.Put, "v2/users/user/permissions/usermanagement");
            await AdminClient.TestSendAsync(HttpMethod.Delete, "v2/users/user/permissions/usermanagement");
        }

        [Fact]
        public async Task UserPermissionNotFound()
        {
            await AdminClient.TestSendAsync(HttpMethod.Put, "v2/users/notexist/permissions/usermanagement", expectedStatusCode: HttpStatusCode.NotFound);
            await AdminClient.TestSendAsync(HttpMethod.Delete, "v2/users/notexist/permissions/usermanagement", expectedStatusCode: HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UserPermissionUnauthorized()
        {
            await DefaultClient.TestSendAsync(HttpMethod.Put, "v2/users/user/permissions/usermanagement", expectedStatusCode: HttpStatusCode.Unauthorized);
            await DefaultClient.TestSendAsync(HttpMethod.Delete, "v2/users/user/permissions/usermanagement", expectedStatusCode: HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UserPermissionForbidden()
        {
            await UserClient.TestSendAsync(HttpMethod.Put, "v2/users/user/permissions/usermanagement", expectedStatusCode: HttpStatusCode.Forbidden);
            await UserClient.TestSendAsync(HttpMethod.Delete, "v2/users/user/permissions/usermanagement", expectedStatusCode: HttpStatusCode.Forbidden);
        }
    }
}

