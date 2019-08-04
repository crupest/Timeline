using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Net.Http;
using Timeline.Controllers;
using Timeline.Entities.Http;
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
            }
        }

        [Fact]
        public async void VerifyTokenTest_GoodToken()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var createTokenResult = await client.CreateUserTokenAsync("admin", "admin");

                var response = await client.PostAsJsonAsync(VerifyTokenUrl, new VerifyTokenRequest { Token = createTokenResult.Token });
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var result = JsonConvert.DeserializeObject<VerifyTokenResponse>(await response.Content.ReadAsStringAsync());
                Assert.NotNull(result.User);
                Assert.Equal(createTokenResult.User.Username, result.User.Username);
                Assert.Equal(createTokenResult.User.Administrator, result.User.Administrator);
            }
        }
    }
}
