using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Controllers;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Tests.Helpers;
using Timeline.Tests.Helpers.Authentication;
using Timeline.Tests.Mock.Data;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests
{
    public class UserUnitTest : IClassFixture<MyWebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public UserUnitTest(MyWebApplicationFactory<Startup> factory, ITestOutputHelper outputHelper)
        {
            _factory = factory.WithTestLogging(outputHelper);
        }

        [Fact]
        public async Task Get_Users_List()
        {
            using (var client = await _factory.CreateClientAsAdmin())
            {
                var res = await client.GetAsync("users");
                // Because tests are running asyncronized. So database may be modified and
                // we can't check the exact user lists at this point. So only check the format.
                res.Should().HaveStatusCodeOk().And.Should().HaveBodyAsJson<UserInfo[]>();
            }
        }

        [Fact]
        public async Task Get_Users_User()
        {
            using (var client = await _factory.CreateClientAsAdmin())
            {
                var res = await client.GetAsync("users/" + MockUsers.UserUsername);
                res.Should().HaveStatusCodeOk()
                    .And.Should().HaveBodyAsJson<UserInfo>()
                    .Which.Should().BeEquivalentTo(MockUsers.UserUserInfo);
            }
        }

        [Fact]
        public async Task Get_Users_404()
        {
            using (var client = await _factory.CreateClientAsAdmin())
            {
                var res = await client.GetAsync("users/usernotexist");
                res.Should().HaveStatusCodeNotFound()
                    .And.Should().HaveBodyAsCommonResponseWithCode(UserController.ErrorCodes.Get_NotExist);
            }
        }

        [Fact]
        public async Task Put_Patch_Delete_User()
        {
            using (var client = await _factory.CreateClientAsAdmin())
            {
                const string username = "putpatchdeleteuser";
                const string password = "password";
                const string url = "users/" + username;

                // Put Invalid Model
                await InvalidModelTestHelpers.TestPutInvalidModel(client, url, new UserPutRequest { Password = null, Administrator = false });
                await InvalidModelTestHelpers.TestPutInvalidModel(client, url, new UserPutRequest { Password = password, Administrator = null });

                async Task CheckAdministrator(bool administrator)
                {
                    var res = await client.GetAsync(url);
                    res.Should().HaveStatusCodeOk()
                        .And.Should().HaveBodyAsJson<UserInfo>()
                        .Which.Administrator.Should().Be(administrator);
                }

                {
                    // Put Bad Username.
                    var res = await client.PutAsJsonAsync("users/dsf fddf", new UserPutRequest
                    {
                        Password = password,
                        Administrator = false
                    });
                    res.Should().HaveStatusCodeBadRequest()
                        .And.Should().HaveBodyAsCommonResponseWithCode(UserController.ErrorCodes.Put_BadUsername);
                }

                {
                    // Put Created.
                    var res = await client.PutAsJsonAsync(url, new UserPutRequest
                    {
                        Password = password,
                        Administrator = false
                    });
                    res.Should().BePutCreated();
                    await CheckAdministrator(false);
                }

                {
                    // Put Modified.
                    var res = await client.PutAsJsonAsync(url, new UserPutRequest
                    {
                        Password = password,
                        Administrator = true
                    });
                    res.Should().BePutModified();
                    await CheckAdministrator(true);
                }

                // Patch Not Exist
                {
                    var res = await client.PatchAsJsonAsync("users/usernotexist", new UserPatchRequest { });
                    res.Should().HaveStatusCodeNotFound()
                        .And.Should().HaveBodyAsCommonResponseWithCode(UserController.ErrorCodes.Patch_NotExist);
                }

                // Patch Success
                {
                    var res = await client.PatchAsJsonAsync(url, new UserPatchRequest { Administrator = false });
                    res.Should().HaveStatusCodeOk();
                    await CheckAdministrator(false);
                }

                // Delete Deleted
                {
                    var res = await client.DeleteAsync(url);
                    res.Should().BeDeleteDeleted();

                    var res2 = await client.GetAsync(url);
                    res2.Should().HaveStatusCodeNotFound();
                }

                // Delete Not Exist
                {
                    var res = await client.DeleteAsync(url);
                    res.Should().BeDeleteNotExist();
                }
            }
        }


        public class ChangePasswordUnitTest : IClassFixture<MyWebApplicationFactory<Startup>>
        {
            private const string url = "userop/changepassword";

            private readonly WebApplicationFactory<Startup> _factory;

            public ChangePasswordUnitTest(MyWebApplicationFactory<Startup> factory, ITestOutputHelper outputHelper)
            {
                _factory = factory.WithTestLogging(outputHelper);
            }


            [Fact]
            public async Task InvalidModel_OldPassword()
            {
                using (var client = await _factory.CreateClientAsUser())
                {
                    await InvalidModelTestHelpers.TestPostInvalidModel(client, url, new ChangePasswordRequest { OldPassword = null, NewPassword = "???" });
                }
            }

            [Fact]
            public async Task InvalidModel_NewPassword()
            {
                using (var client = await _factory.CreateClientAsUser())
                {
                    await InvalidModelTestHelpers.TestPostInvalidModel(client, url, new ChangePasswordRequest { OldPassword = "???", NewPassword = null });
                }
            }

            [Fact]
            public async Task BadOldPassword()
            {
                using (var client = await _factory.CreateClientAsUser())
                {
                    var res = await client.PostAsJsonAsync(url, new ChangePasswordRequest { OldPassword = "???", NewPassword = "???" });
                    res.Should().HaveStatusCodeBadRequest()
                        .And.Should().HaveBodyAsCommonResponseWithCode(UserController.ErrorCodes.ChangePassword_BadOldPassword);
                }
            }

            [Fact]
            public async Task Success()
            {
                const string username = "changepasswordtest";
                const string password = "password";

                // create a new user to avoid interference
                using (var client = await _factory.CreateClientAsAdmin())
                {
                    var res = await client.PutAsJsonAsync("users/" + username, new UserPutRequest { Password = password, Administrator = false });
                    res.Should().BePutCreated();
                }

                using (var client = await _factory.CreateClientWithCredential(username, password))
                {
                    const string newPassword = "new";
                    var res = await client.PostAsJsonAsync(url, new ChangePasswordRequest { OldPassword = password, NewPassword = newPassword });
                    res.Should().HaveStatusCodeOk();
                    await client.CreateUserTokenAsync(username, newPassword);
                }
            }
        }
    }
}
