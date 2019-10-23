using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Tests.Helpers;
using Timeline.Tests.Helpers.Authentication;
using Timeline.Tests.Mock.Data;
using Xunit;
using static Timeline.ErrorCodes.Http.User;

namespace Timeline.Tests.IntegratedTests
{
    public class UserTest : IClassFixture<WebApplicationFactory<Startup>>, IDisposable
    {
        private readonly TestApplication _testApp;
        private readonly WebApplicationFactory<Startup> _factory;

        public UserTest(WebApplicationFactory<Startup> factory)
        {
            _testApp = new TestApplication(factory);
            _factory = _testApp.Factory;
        }

        public void Dispose()
        {
            _testApp.Dispose();
        }

        [Fact]
        public async Task Get_List_Success()
        {
            using var client = await _factory.CreateClientAsAdmin();
            var res = await client.GetAsync("users");
            res.Should().HaveStatusCode(200)
                .And.Should().HaveJsonBody<UserInfo[]>()
                .Which.Should().BeEquivalentTo(MockUser.UserInfoList);
        }

        [Fact]
        public async Task Get_Single_Success()
        {
            using var client = await _factory.CreateClientAsAdmin();
            var res = await client.GetAsync("users/" + MockUser.User.Username);
            res.Should().HaveStatusCode(200)
                .And.Should().HaveJsonBody<UserInfo>()
                .Which.Should().BeEquivalentTo(MockUser.User.Info);
        }

        [Fact]
        public async Task Get_InvalidModel()
        {
            using var client = await _factory.CreateClientAsAdmin();
            var res = await client.GetAsync("users/aaa!a");
            res.Should().BeInvalidModel();
        }

        [Fact]
        public async Task Get_Users_404()
        {
            using var client = await _factory.CreateClientAsAdmin();
            var res = await client.GetAsync("users/usernotexist");
            res.Should().HaveStatusCode(404)
                .And.Should().HaveCommonBody()
                .Which.Code.Should().Be(Get.NotExist);
        }

        public static IEnumerable<object[]> Put_InvalidModel_Data()
        {
            yield return new object[] { "aaa", null, false };
            yield return new object[] { "aaa", "p", null };
            yield return new object[] { "aa!a", "p", false };
        }

        [Theory]
        [MemberData(nameof(Put_InvalidModel_Data))]
        public async Task Put_InvalidModel(string username, string password, bool? administrator)
        {
            using var client = await _factory.CreateClientAsAdmin();
            (await client.PutAsJsonAsync("users/" + username,
                new UserPutRequest { Password = password, Administrator = administrator }))
                .Should().BeInvalidModel();
        }

        private async Task CheckAdministrator(HttpClient client, string username, bool administrator)
        {
            var res = await client.GetAsync("users/" + username);
            res.Should().HaveStatusCode(200)
                .And.Should().HaveJsonBody<UserInfo>()
                .Which.Administrator.Should().Be(administrator);
        }

        [Fact]
        public async Task Put_Modiefied()
        {
            using var client = await _factory.CreateClientAsAdmin();
            var res = await client.PutAsJsonAsync("users/" + MockUser.User.Username, new UserPutRequest
            {
                Password = "password",
                Administrator = false
            });
            res.Should().BePut(false);
            await CheckAdministrator(client, MockUser.User.Username, false);
        }

        [Fact]
        public async Task Put_Created()
        {
            using var client = await _factory.CreateClientAsAdmin();
            const string username = "puttest";
            const string url = "users/" + username;

            var res = await client.PutAsJsonAsync(url, new UserPutRequest
            {
                Password = "password",
                Administrator = false
            });
            res.Should().BePut(true);
            await CheckAdministrator(client, username, false);
        }

        [Fact]
        public async Task Patch_NotExist()
        {
            using var client = await _factory.CreateClientAsAdmin();
            var res = await client.PatchAsJsonAsync("users/usernotexist", new UserPatchRequest { });
            res.Should().HaveStatusCode(404)
                .And.Should().HaveCommonBody()
                .Which.Code.Should().Be(Patch.NotExist);
        }

        [Fact]
        public async Task Patch_InvalidModel()
        {
            using var client = await _factory.CreateClientAsAdmin();
            var res = await client.PatchAsJsonAsync("users/aaa!a", new UserPatchRequest { });
            res.Should().BeInvalidModel();
        }

        [Fact]
        public async Task Patch_Success()
        {
            using var client = await _factory.CreateClientAsAdmin();
            {
                var res = await client.PatchAsJsonAsync("users/" + MockUser.User.Username,
                    new UserPatchRequest { Administrator = false });
                res.Should().HaveStatusCode(200);
                await CheckAdministrator(client, MockUser.User.Username, false);
            }
        }

        [Fact]
        public async Task Delete_InvalidModel()
        {
            using var client = await _factory.CreateClientAsAdmin();
            var url = "users/aaa!a";
            var res = await client.DeleteAsync(url);
            res.Should().BeInvalidModel();
        }

        [Fact]
        public async Task Delete_Deleted()
        {
            using var client = await _factory.CreateClientAsAdmin();
            var url = "users/" + MockUser.User.Username;
            var res = await client.DeleteAsync(url);
            res.Should().BeDelete(true);

            var res2 = await client.GetAsync(url);
            res2.Should().HaveStatusCode(404);
        }

        [Fact]
        public async Task Delete_NotExist()
        {
            using var client = await _factory.CreateClientAsAdmin();
            var res = await client.DeleteAsync("users/usernotexist");
            res.Should().BeDelete(false);
        }

        private const string changeUsernameUrl = "userop/changeusername";

        public static IEnumerable<object[]> Op_ChangeUsername_InvalidModel_Data()
        {
            yield return new[] { null, "uuu" };
            yield return new[] { "uuu", null };
            yield return new[] { "a!a", "uuu" };
            yield return new[] { "uuu", "a!a" };
        }

        [Theory]
        [MemberData(nameof(Op_ChangeUsername_InvalidModel_Data))]
        public async Task Op_ChangeUsername_InvalidModel(string oldUsername, string newUsername)
        {
            using var client = await _factory.CreateClientAsAdmin();
            (await client.PostAsJsonAsync(changeUsernameUrl,
                new ChangeUsernameRequest { OldUsername = oldUsername, NewUsername = newUsername }))
                .Should().BeInvalidModel();
        }

        [Fact]
        public async Task Op_ChangeUsername_UserNotExist()
        {
            using var client = await _factory.CreateClientAsAdmin();
            var res = await client.PostAsJsonAsync(changeUsernameUrl,
                new ChangeUsernameRequest { OldUsername = "usernotexist", NewUsername = "newUsername" });
            res.Should().HaveStatusCode(400)
                .And.Should().HaveCommonBody()
                .Which.Code.Should().Be(Op.ChangeUsername.NotExist);
        }

        [Fact]
        public async Task Op_ChangeUsername_UserAlreadyExist()
        {
            using var client = await _factory.CreateClientAsAdmin();
            var res = await client.PostAsJsonAsync(changeUsernameUrl,
                new ChangeUsernameRequest { OldUsername = MockUser.User.Username, NewUsername = MockUser.Admin.Username });
            res.Should().HaveStatusCode(400)
                .And.Should().HaveCommonBody()
                .Which.Code.Should().Be(Op.ChangeUsername.AlreadyExist);
        }

        [Fact]
        public async Task Op_ChangeUsername_Success()
        {
            using var client = await _factory.CreateClientAsAdmin();
            const string newUsername = "hahaha";
            var res = await client.PostAsJsonAsync(changeUsernameUrl,
                new ChangeUsernameRequest { OldUsername = MockUser.User.Username, NewUsername = newUsername });
            res.Should().HaveStatusCode(200);
            await client.CreateUserTokenAsync(newUsername, MockUser.User.Password);
        }

        private const string changePasswordUrl = "userop/changepassword";

        public static IEnumerable<object[]> Op_ChangePassword_InvalidModel_Data()
        {
            yield return new[] { null, "ppp" };
            yield return new[] { "ppp", null };
        }

        [Theory]
        [MemberData(nameof(Op_ChangePassword_InvalidModel_Data))]
        public async Task Op_ChangePassword_InvalidModel(string oldPassword, string newPassword)
        {
            using var client = await _factory.CreateClientAsUser();
            (await client.PostAsJsonAsync(changePasswordUrl,
                new ChangePasswordRequest { OldPassword = oldPassword, NewPassword = newPassword }))
                .Should().BeInvalidModel();
        }

        [Fact]
        public async Task Op_ChangePassword_BadOldPassword()
        {
            using var client = await _factory.CreateClientAsUser();
            var res = await client.PostAsJsonAsync(changePasswordUrl, new ChangePasswordRequest { OldPassword = "???", NewPassword = "???" });
            res.Should().HaveStatusCode(400)
                .And.Should().HaveCommonBody()
                .Which.Code.Should().Be(Op.ChangePassword.BadOldPassword);
        }

        [Fact]
        public async Task Op_ChangePassword_Success()
        {
            using var client = await _factory.CreateClientAsUser();
            const string newPassword = "new";
            var res = await client.PostAsJsonAsync(changePasswordUrl,
                new ChangePasswordRequest { OldPassword = MockUser.User.Password, NewPassword = newPassword });
            res.Should().HaveStatusCode(200);
            await _factory.CreateDefaultClient() // don't use client above, because it sets authorization header
                .CreateUserTokenAsync(MockUser.User.Username, newPassword);
        }
    }
}
