using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Net.Http;
using Timeline.Controllers;
using Timeline.Entities;
using Timeline.Entities.Http;
using Timeline.Models;
using Timeline.Services;
using Timeline.Tests.Helpers;
using Timeline.Tests.Helpers.Authentication;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests
{
    public class TokenUnitTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private const string CreateTokenUrl = "token/create";
        private const string VerifyTokenUrl = "token/verify";

        private readonly WebApplicationFactory<Startup> _factory;

        public TokenUnitTest(WebApplicationFactory<Startup> factory, ITestOutputHelper outputHelper)
        {
            _factory = factory.WithTestConfig(outputHelper);
        }

        [Fact]
        public async void CreateTokenTest_UserNotExist()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var response = await client.PostAsJsonAsync(CreateTokenUrl, new CreateTokenRequest { Username = "usernotexist", Password = "???" });
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                var body = await response.ReadBodyAsJson<CommonResponse>();
                Assert.Equal(TokenController.ErrorCodes.Create_UserNotExist, body.Code);
            }
        }

        [Fact]
        public async void CreateTokenTest_BadPassword()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var response = await client.PostAsJsonAsync(CreateTokenUrl, new CreateTokenRequest { Username = "user", Password = "???" });
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                var body = await response.ReadBodyAsJson<CommonResponse>();
                Assert.Equal(TokenController.ErrorCodes.Create_BadPassword, body.Code);
            }
        }

        [Fact]
        public async void CreateTokenTest_BadExpireOffset()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var response = await client.PostAsJsonAsync(CreateTokenUrl, new CreateTokenRequest { Username = "???", Password = "???", ExpireOffset = -1000 });
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                var body = await response.ReadBodyAsJson<CommonResponse>();
                Assert.Equal(TokenController.ErrorCodes.Create_BadExpireOffset, body.Code);
            }
        }

        [Fact]
        public async void CreateTokenTest_Success()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var response = await client.PostAsJsonAsync(CreateTokenUrl, new CreateTokenRequest { Username = "user", Password = "user" });
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var body = await response.ReadBodyAsJson<CreateTokenResponse>();
                Assert.NotEmpty(body.Token);
                Assert.Equal(TestMockUsers.MockUserInfos.Where(u => u.Username == "user").Single(), body.User, UserInfoComparers.EqualityComparer);
            }
        }

        [Fact]
        public async void VerifyTokenTest_BadToken()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var response = await client.PostAsJsonAsync(VerifyTokenUrl, new VerifyTokenRequest { Token = "bad token hahaha" });
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                var body = await response.ReadBodyAsJson<CommonResponse>();
                Assert.Equal(TokenController.ErrorCodes.Verify_BadToken, body.Code);
            }
        }

        [Fact]
        public async void VerifyTokenTest_BadVersion_AND_UserNotExist()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                using (var scope = _factory.Server.Host.Services.CreateScope()) // UserService is scoped.
                {
                    // create a user for test
                    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                    const string username = "verifytokentest0";
                    const string password = "12345678";

                    await userService.PutUser(username, password, false);

                    // create a token
                    var token = (await client.CreateUserTokenAsync(username, password)).Token;

                    // increase version
                    await userService.PatchUser(username, null, null);

                    // test against bad version
                    var response = await client.PostAsJsonAsync(VerifyTokenUrl, new VerifyTokenRequest { Token = token });
                    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                    var body = await response.ReadBodyAsJson<CommonResponse>();
                    Assert.Equal(TokenController.ErrorCodes.Verify_BadVersion, body.Code);

                    // create another token
                    var token2 = (await client.CreateUserTokenAsync(username, password)).Token;

                    // delete user
                    await userService.DeleteUser(username);

                    // test against user not exist
                    var response2 = await client.PostAsJsonAsync(VerifyTokenUrl, new VerifyTokenRequest { Token = token });
                    Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
                    var body2 = await response2.ReadBodyAsJson<CommonResponse>();
                    Assert.Equal(TokenController.ErrorCodes.Verify_UserNotExist, body2.Code);
                }
            }
        }

        [Fact]
        public async void VerifyTokenTest_Success()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var createTokenResult = await client.CreateUserTokenAsync("admin", "admin");
                var response = await client.PostAsJsonAsync(VerifyTokenUrl, new VerifyTokenRequest { Token = createTokenResult.Token });
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var body = JsonConvert.DeserializeObject<VerifyTokenResponse>(await response.Content.ReadAsStringAsync());
                Assert.Equal(TestMockUsers.MockUserInfos.Where(u => u.Username == "user").Single(), body.User, UserInfoComparers.EqualityComparer);
            }
        }
    }
}
