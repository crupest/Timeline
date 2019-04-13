using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using Timeline.Entities;
using Timeline.Tests.Helpers;
using Timeline.Tests.Helpers.Authentication;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests
{
    public class JwtTokenUnitTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private const string CreateTokenUrl = "User/CreateToken";
        private const string ValidateTokenUrl = "User/ValidateToken";

        private readonly WebApplicationFactory<Startup> _factory;

        public JwtTokenUnitTest(WebApplicationFactory<Startup> factory, ITestOutputHelper outputHelper)
        {
            _factory = factory.WithTestConfig(outputHelper);
        }

        [Fact]
        public async void CreateTokenTest_BadCredential()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var response = await client.PostAsJsonAsync(CreateTokenUrl, new CreateTokenRequest { Username = "???", Password = "???" });
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var result = JsonConvert.DeserializeObject<CreateTokenResponse>(await response.Content.ReadAsStringAsync());
                Assert.False(result.Success);
                Assert.Null(result.Token);
                Assert.Null(result.UserInfo);
            }
        }

        [Fact]
        public async void CreateTokenTest_GoodCredential()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var response = await client.PostAsJsonAsync(CreateTokenUrl, new CreateTokenRequest { Username = "user", Password = "user" });
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var result = JsonConvert.DeserializeObject<CreateTokenResponse>(await response.Content.ReadAsStringAsync());
                Assert.True(result.Success);
                Assert.NotNull(result.Token);
                Assert.NotNull(result.UserInfo);
            }
        }

        [Fact]
        public async void ValidateTokenTest_BadToken()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var response = await client.PostAsJsonAsync(ValidateTokenUrl, new TokenValidationRequest { Token = "bad token hahaha" });

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var validationInfo = JsonConvert.DeserializeObject<TokenValidationResponse>(await response.Content.ReadAsStringAsync());

                Assert.False(validationInfo.IsValid);
                Assert.Null(validationInfo.UserInfo);
            }
        }

        [Fact]
        public async void ValidateTokenTest_GoodToken()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var createTokenResult = await client.CreateUserTokenAsync("admin", "admin");

                var response = await client.PostAsJsonAsync(ValidateTokenUrl, new TokenValidationRequest { Token = createTokenResult.Token });
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var result = JsonConvert.DeserializeObject<TokenValidationResponse>(await response.Content.ReadAsStringAsync());

                Assert.True(result.IsValid);
                Assert.NotNull(result.UserInfo);
                Assert.Equal(createTokenResult.UserInfo.Username, result.UserInfo.Username);
                Assert.Equal(createTokenResult.UserInfo.Roles, result.UserInfo.Roles);
            }
        }
    }
}
