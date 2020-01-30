using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Models.Http;
using Timeline.Services;
using Timeline.Tests.Helpers;
using Xunit;

namespace Timeline.Tests.IntegratedTests
{
    public class TokenTest : IntegratedTestBase
    {
        private const string CreateTokenUrl = "token/create";
        private const string VerifyTokenUrl = "token/verify";

        public TokenTest(WebApplicationFactory<Startup> factory)
            : base(factory)
        {

        }

        private static async Task<CreateTokenResponse> CreateUserTokenAsync(HttpClient client, string username, string password, int? expireOffset = null)
        {
            var response = await client.PostAsJsonAsync(CreateTokenUrl, new CreateTokenRequest { Username = username, Password = password, Expire = expireOffset });
            return response.Should().HaveStatusCode(200)
                .And.HaveJsonBody<CreateTokenResponse>().Which;
        }

        public static IEnumerable<object[]> CreateToken_InvalidModel_Data()
        {
            yield return new[] { null, "p", null };
            yield return new[] { "u", null, null };
            yield return new object[] { "u", "p", 2000 };
            yield return new object[] { "u", "p", -1 };
        }

        [Theory]
        [MemberData(nameof(CreateToken_InvalidModel_Data))]
        public async Task CreateToken_InvalidModel(string username, string password, int expire)
        {
            using var client = await CreateDefaultClient();
            (await client.PostAsJsonAsync(CreateTokenUrl, new CreateTokenRequest
            {
                Username = username,
                Password = password,
                Expire = expire
            })).Should().BeInvalidModel();
        }

        public static IEnumerable<object[]> CreateToken_UserCredential_Data()
        {
            yield return new[] { "usernotexist", "p" };
            yield return new[] { "user1", "???" };
        }

        [Theory]
        [MemberData(nameof(CreateToken_UserCredential_Data))]
        public async void CreateToken_UserCredential(string username, string password)
        {
            using var client = await CreateDefaultClient();
            var response = await client.PostAsJsonAsync(CreateTokenUrl,
                new CreateTokenRequest { Username = username, Password = password });
            response.Should().HaveStatusCode(400)
                .And.HaveCommonBody()
                .Which.Code.Should().Be(ErrorCodes.TokenController.Create_BadCredential);
        }

        [Fact]
        public async Task CreateToken_Success()
        {
            using var client = await CreateDefaultClient();
            var response = await client.PostAsJsonAsync(CreateTokenUrl,
                new CreateTokenRequest { Username = "user1", Password = "user1pw" });
            var body = response.Should().HaveStatusCode(200)
               .And.HaveJsonBody<CreateTokenResponse>().Which;
            body.Token.Should().NotBeNullOrWhiteSpace();
            body.User.Should().BeEquivalentTo(UserInfoForAdminList[1]);
        }

        [Fact]
        public async Task VerifyToken_InvalidModel()
        {
            using var client = await CreateDefaultClient();
            (await client.PostAsJsonAsync(VerifyTokenUrl,
                new VerifyTokenRequest { Token = null })).Should().BeInvalidModel();
        }

        [Fact]
        public async Task VerifyToken_BadFormat()
        {
            using var client = await CreateDefaultClient();
            var response = await client.PostAsJsonAsync(VerifyTokenUrl,
                new VerifyTokenRequest { Token = "bad token hahaha" });
            response.Should().HaveStatusCode(400)
                 .And.HaveCommonBody()
                 .Which.Code.Should().Be(ErrorCodes.TokenController.Verify_BadFormat);
        }

        [Fact]
        public async Task VerifyToken_OldVersion()
        {
            using var client = await CreateDefaultClient();
            var token = (await CreateUserTokenAsync(client, "user1", "user1pw")).Token;

            using (var scope = Factory.Services.CreateScope()) // UserService is scoped.
            {
                // create a user for test
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                await userService.ModifyUser("user1", new User { Password = "user1pw" });
            }

            (await client.PostAsJsonAsync(VerifyTokenUrl,
                new VerifyTokenRequest { Token = token }))
                .Should().HaveStatusCode(400)
                .And.HaveCommonBody()
                .Which.Code.Should().Be(ErrorCodes.TokenController.Verify_OldVersion);
        }

        [Fact]
        public async Task VerifyToken_UserNotExist()
        {
            using var client = await CreateDefaultClient();
            var token = (await CreateUserTokenAsync(client, "user1", "user1pw")).Token;

            using (var scope = Factory.Server.Host.Services.CreateScope()) // UserService is scoped.
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                await userService.DeleteUser("user1");
            }

            (await client.PostAsJsonAsync(VerifyTokenUrl,
                new VerifyTokenRequest { Token = token }))
                .Should().HaveStatusCode(400)
                .And.HaveCommonBody()
                .Which.Code.Should().Be(ErrorCodes.TokenController.Verify_UserNotExist);
        }

        //[Fact]
        //public async Task VerifyToken_Expired()
        //{
        //    using (var client = await CreateClientWithNoAuth())
        //    {
        //        // I can only control the token expired time but not current time
        //        // because verify logic is encapsuled in other library.
        //        var mockClock = _factory.GetTestClock();
        //        mockClock.MockCurrentTime = DateTime.Now - TimeSpan.FromDays(2);
        //        var token = (await client.CreateUserTokenAsync(MockUsers.UserUsername, MockUsers.UserPassword, 1)).Token;
        //        var response = await client.PostAsJsonAsync(VerifyTokenUrl,
        //            new VerifyTokenRequest { Token = token });
        //        response.Should().HaveStatusCodeBadRequest()
        //            .And.Should().HaveBodyAsCommonResponseWithCode(TokenController.ErrorCodes.Verify_Expired);
        //        mockClock.MockCurrentTime = null;
        //    }
        //}

        [Fact]
        public async Task VerifyToken_Success()
        {
            using var client = await CreateDefaultClient();
            var createTokenResult = await CreateUserTokenAsync(client, "user1", "user1pw");
            var response = await client.PostAsJsonAsync(VerifyTokenUrl,
                new VerifyTokenRequest { Token = createTokenResult.Token });
            response.Should().HaveStatusCode(200)
                .And.HaveJsonBody<VerifyTokenResponse>()
                .Which.User.Should().BeEquivalentTo(UserInfoForAdminList[1]);
        }
    }
}
