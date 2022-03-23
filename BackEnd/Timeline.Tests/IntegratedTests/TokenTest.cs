using FluentAssertions;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Models.Http;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests.IntegratedTests
{
    public class TokenTest : IntegratedTestBase
    {
        public TokenTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {

        }

        private const string CreateTokenUrl = "token/create";
        private const string VerifyTokenUrl = "token/verify";

        private static async Task<HttpCreateTokenResponse> CreateUserTokenAsync(HttpClient client, string username, string password, int? expireOffset = null)
        {
            return await client.TestPostAsync<HttpCreateTokenResponse>(CreateTokenUrl, new HttpCreateTokenRequest { Username = username, Password = password, Expire = expireOffset });
        }

        public static IEnumerable<object?[]> CreateToken_InvalidModel_Data()
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
            await client.TestPostAssertInvalidModelAsync(CreateTokenUrl, new HttpCreateTokenRequest
            {
                Username = username,
                Password = password,
                Expire = expire
            });
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
            await client.TestPostAssertErrorAsync(CreateTokenUrl,
                new HttpCreateTokenRequest { Username = username, Password = password },
                errorCode: ErrorCodes.TokenController.CreateBadCredential);
        }

        [Fact]
        public async Task CreateToken_Success()
        {
            using var client = await CreateDefaultClient();
            var body = await client.TestPostAsync<HttpCreateTokenResponse>(CreateTokenUrl,
                new HttpCreateTokenRequest { Username = "user1", Password = "user1pw" });
            body.Token.Should().NotBeNullOrWhiteSpace();
            body.User.Should().BeEquivalentTo(await client.GetUserAsync("user1"));
        }

        [Fact]
        public async Task VerifyToken_InvalidModel()
        {
            using var client = await CreateDefaultClient();
            await client.TestPostAssertInvalidModelAsync(VerifyTokenUrl, new HttpVerifyTokenRequest { Token = null! });
        }

        [Fact]
        public async Task VerifyToken_Invalid()
        {
            using var client = await CreateDefaultClient();
            await client.TestPostAssertErrorAsync(VerifyTokenUrl,
                new HttpVerifyTokenRequest { Token = "bad token hahaha" },
                errorCode: ErrorCodes.TokenController.VerifyInvalid);
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
            var body = await client.TestPostAsync<HttpVerifyTokenResponse>(VerifyTokenUrl,
                new HttpVerifyTokenRequest { Token = createTokenResult.Token });
            body.User.Should().BeEquivalentTo(await client.GetUserAsync("user1"));
        }
    }
}
