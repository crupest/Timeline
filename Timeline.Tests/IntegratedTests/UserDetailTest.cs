using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;
using Timeline.Models.Http;
using Timeline.Tests.Helpers;
using Xunit;

namespace Timeline.Tests.IntegratedTests
{
    public class UserDetailTest : IntegratedTestBase
    {
        public UserDetailTest(WebApplicationFactory<Startup> factory)
            : base(factory)
        {

        }

        [Fact]
        public async Task PermissionTest()
        {
            { // unauthorize
                using var client = await CreateClientWithNoAuth();
                { // GET
                    var res = await client.GetAsync($"users/{MockUser.User.Username}/nickname");
                    res.Should().HaveStatusCode(HttpStatusCode.OK);
                }
                { // PUT
                    var res = await client.PutStringAsync($"users/{MockUser.User.Username}/nickname", "aaa");
                    res.Should().HaveStatusCode(HttpStatusCode.Unauthorized);
                }
                { // DELETE
                    var res = await client.DeleteAsync($"users/{MockUser.User.Username}/nickname");
                    res.Should().HaveStatusCode(HttpStatusCode.Unauthorized);
                }
            }
            { // user
                using var client = await CreateClientAsUser();
                { // GET
                    var res = await client.GetAsync($"users/{MockUser.User.Username}/nickname");
                    res.Should().HaveStatusCode(HttpStatusCode.OK);
                }
                { // PUT self
                    var res = await client.PutStringAsync($"users/{MockUser.User.Username}/nickname", "aaa");
                    res.Should().HaveStatusCode(HttpStatusCode.OK);
                }
                { // PUT other
                    var res = await client.PutStringAsync($"users/{MockUser.Admin.Username}/nickname", "aaa");
                    res.Should().HaveStatusCode(HttpStatusCode.Forbidden);
                }
                { // DELETE self
                    var res = await client.DeleteAsync($"users/{MockUser.User.Username}/nickname");
                    res.Should().HaveStatusCode(HttpStatusCode.OK);
                }
                { // DELETE other
                    var res = await client.DeleteAsync($"users/{MockUser.Admin.Username}/nickname");
                    res.Should().HaveStatusCode(HttpStatusCode.Forbidden);
                }
            }
            { // user
                using var client = await CreateClientAsAdmin();
                { // PUT other
                    var res = await client.PutStringAsync($"users/{MockUser.User.Username}/nickname", "aaa");
                    res.Should().HaveStatusCode(HttpStatusCode.OK);
                }
                { // DELETE other
                    var res = await client.DeleteAsync($"users/{MockUser.User.Username}/nickname");
                    res.Should().HaveStatusCode(HttpStatusCode.OK);
                }
            }
        }

        [Fact]
        public async Task FunctionTest()
        {
            var url = $"users/{MockUser.User.Username}/nickname";
            var userNotExistUrl = "users/usernotexist/nickname";
            {
                using var client = await CreateClientAsUser();
                {
                    var res = await client.GetAsync(userNotExistUrl);
                    res.Should().HaveStatusCode(HttpStatusCode.NotFound)
                        .And.HaveCommonBody()
                        .Which.Code.Should().Be(ErrorCodes.UserCommon.NotExist);

                }
                {
                    var res = await client.GetAsync(url);
                    res.Should().HaveStatusCode(HttpStatusCode.OK);
                    res.Content.Headers.ContentType.Should().Be(new MediaTypeHeaderValue(MediaTypeNames.Text.Plain) { CharSet = "utf-8" });
                    var body = await res.Content.ReadAsStringAsync();
                    body.Should().Be(MockUser.User.Username);
                }
                {
                    var res = await client.PutStringAsync(url, "");
                    res.Should().BeInvalidModel();
                }
                {
                    var res = await client.PutStringAsync(url, new string('a', 11));
                    res.Should().BeInvalidModel();
                }
                var nickname1 = "nnn";
                var nickname2 = "nn2";
                {
                    var res = await client.PutStringAsync(url, nickname1);
                    res.Should().HaveStatusCode(HttpStatusCode.OK);
                    (await client.GetStringAsync(url)).Should().Be(nickname1);
                }
                {
                    var res = await client.PutStringAsync(url, nickname2);
                    res.Should().HaveStatusCode(HttpStatusCode.OK);
                    (await client.GetStringAsync(url)).Should().Be(nickname2);
                }
                {
                    var res = await client.DeleteAsync(url);
                    res.Should().HaveStatusCode(HttpStatusCode.OK);
                    (await client.GetStringAsync(url)).Should().Be(MockUser.User.Username);
                }
                {
                    var res = await client.DeleteAsync(url);
                    res.Should().HaveStatusCode(HttpStatusCode.OK);
                }
            }
            {
                using var client = await CreateClientAsAdmin();
                {
                    var res = await client.PutStringAsync(userNotExistUrl, "aaa");
                    res.Should().HaveStatusCode(HttpStatusCode.BadRequest)
                        .And.HaveCommonBody()
                        .Which.Code.Should().Be(ErrorCodes.UserCommon.NotExist);
                }
                {
                    var res = await client.DeleteAsync(userNotExistUrl);
                    res.Should().HaveStatusCode(HttpStatusCode.BadRequest)
                        .And.HaveCommonBody()
                        .Which.Code.Should().Be(ErrorCodes.UserCommon.NotExist);
                }
                var nickname = "nnn";
                {
                    var res = await client.PutStringAsync(url, nickname);
                    res.Should().HaveStatusCode(HttpStatusCode.OK);
                    (await client.GetStringAsync(url)).Should().Be(nickname);
                }
                {
                    var res = await client.DeleteAsync(url);
                    res.Should().HaveStatusCode(HttpStatusCode.OK);
                    (await client.GetStringAsync(url)).Should().Be(MockUser.User.Username);
                }
            }
        }
    }
}
