using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Timeline.Controllers;
using Timeline.Models.Http;
using Timeline.Services;
using Timeline.Tests.Helpers;
using Timeline.Tests.Helpers.Authentication;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests
{
    public class TokenUnitTest : IClassFixture<MyWebApplicationFactory<Startup>>
    {
        private const string CreateTokenUrl = "token/create";
        private const string VerifyTokenUrl = "token/verify";

        private readonly WebApplicationFactory<Startup> _factory;

        public TokenUnitTest(MyWebApplicationFactory<Startup> factory, ITestOutputHelper outputHelper)
        {
            _factory = factory.WithTestLogging(outputHelper);
        }

        [Fact]
        public async void CreateToken_MissingUsername()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                await InvalidModelTestHelpers.TestPostInvalidModel(client, CreateTokenUrl,
                    new CreateTokenRequest { Username = null, Password = "user" });
            }
        }

        [Fact]
        public async void CreateToken_InvalidModel_MissingPassword()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                await InvalidModelTestHelpers.TestPostInvalidModel(client, CreateTokenUrl,
                    new CreateTokenRequest { Username = "user", Password = null });
            }
        }

        [Fact]
        public async void CreateToken_InvalidModel_BadExpireOffset()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                await InvalidModelTestHelpers.TestPostInvalidModel(client, CreateTokenUrl,
                    new CreateTokenRequest { Username = "user", Password = "user", ExpireOffset = -1000 });
            }
        }

        [Fact]
        public async void CreateToken_UserNotExist()
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
        public async void CreateToken_BadPassword()
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
        public async void CreateToken_Success()
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
        public async void VerifyToken_InvalidModel_MissingToken()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                await InvalidModelTestHelpers.TestPostInvalidModel(client, VerifyTokenUrl,
                    new VerifyTokenRequest { Token = null });
            }
        }

        [Fact]
        public async void VerifyToken_BadToken()
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
        public async void VerifyToken_BadVersion_AND_UserNotExist()
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
        public async void VerifyToken_Expired()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                // I can only control the token expired time but not current time
                // because verify logic is encapsuled in other library.
                var mockClock = _factory.GetTestClock();
                mockClock.MockCurrentTime = DateTime.Now - TimeSpan.FromDays(2);
                var token = (await client.CreateUserTokenAsync("user", "user", 1)).Token;
                var response = await client.PostAsJsonAsync(VerifyTokenUrl, new VerifyTokenRequest { Token = token });
                var body = await response.ReadBodyAsJson<CommonResponse>();
                Assert.Equal(TokenController.ErrorCodes.Verify_Expired, body.Code);
                mockClock.MockCurrentTime = null;
            }
        }

        [Fact]
        public async void VerifyToken_Success()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var createTokenResult = await client.CreateUserTokenAsync("user", "user");
                var response = await client.PostAsJsonAsync(VerifyTokenUrl, new VerifyTokenRequest { Token = createTokenResult.Token });
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var body = JsonConvert.DeserializeObject<VerifyTokenResponse>(await response.Content.ReadAsStringAsync());
                Assert.Equal(TestMockUsers.MockUserInfos.Where(u => u.Username == "user").Single(), body.User, UserInfoComparers.EqualityComparer);
            }
        }
    }
}
