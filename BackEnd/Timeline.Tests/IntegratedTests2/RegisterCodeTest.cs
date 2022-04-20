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
    public class RegisterCodeTest : IntegratedTestBase
    {
        public RegisterCodeTest(ITestOutputHelper testOutput) : base(testOutput)
        {
        }

        [Fact]
        public async Task RegisterTest()
        {
            await DefaultClient.TestJsonSendAsync(HttpMethod.Post, "v2/register", new HttpRegisterCodeRegisterRequest
            {
                Username = "hello",
                Password = "passwd",
            }, expectedStatusCode: HttpStatusCode.UnprocessableEntity);

            await DefaultClient.TestJsonSendAsync(HttpMethod.Post, "v2/register", new HttpRegisterCodeRegisterRequest
            {
                Username = "hello",
                Password = "passwd",
                RegisterCode = "invalidone"
            }, expectedStatusCode: HttpStatusCode.UnprocessableEntity);

            var b = await UserClient.TestJsonSendAsync<HttpRegisterCode>(HttpMethod.Get, "v2/users/user/registercode");
            b.RegisterCode.Should().BeNull();

            var a = await UserClient.TestJsonSendAsync<HttpRegisterCode>(HttpMethod.Post, "v2/users/user/renewregistercode");
            a.RegisterCode.Should().NotBeNull();

            var c = await UserClient.TestJsonSendAsync<HttpRegisterCode>(HttpMethod.Get, "v2/users/user/registercode");
            c.RegisterCode.Should().NotBeNull().And.Be(a.RegisterCode);

            var d = await DefaultClient.TestJsonSendAsync<HttpUser>(HttpMethod.Post, "v2/register", new HttpRegisterCodeRegisterRequest
            {
                Username = "hello",
                Password = "passwd",
                RegisterCode = a.RegisterCode!
            });
            d.Username.Should().Be("hello");
        }

        [Fact]
        public async Task PermissionTest()
        {
            await TestOnlySelfAndAdminCanCall(HttpMethod.Get, "v2/users/user/registercode", "v2/users/admin/registercode", null);
            await TestOnlySelfAndAdminCanCall(HttpMethod.Post, "v2/users/user/renewregistercode", "v2/users/admin/renewregistercode", null);
        }
    }
}

