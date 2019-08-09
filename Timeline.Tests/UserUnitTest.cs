using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
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
                Assert.Equal(HttpStatusCode.OK, res.StatusCode);

                // Because tests are running asyncronized. So database may be modified and
                // we can't check the exact user lists at this point. So only check the format.

                // var users = (await res.ReadBodyAsJson<UserInfo[]>()).ToList();
                // users.Sort(UserInfoComparers.Comparer);
                // Assert.Equal(MockUsers.UserInfos, users, UserInfoComparers.EqualityComparer);
                await res.ReadBodyAsJson<UserInfo[]>();
            }
        }

        [Fact]
        public async Task Get_Users_User()
        {
            using (var client = await _factory.CreateClientAsAdmin())
            {
                var res = await client.GetAsync("users/" + MockUsers.UserUsername);
                res.AssertOk();
                var user = await res.ReadBodyAsJson<UserInfo>();
                Assert.Equal(MockUsers.UserUserInfo, user, UserInfoComparers.EqualityComparer);
            }
        }

        [Fact]
        public async Task Get_Users_404()
        {
            using (var client = await _factory.CreateClientAsAdmin())
            {
                var res = await client.GetAsync("users/usernotexist");
                res.AssertNotFound();
                var body = await res.ReadBodyAsJson<CommonResponse>();
                Assert.Equal(UserController.ErrorCodes.Get_NotExist, body.Code);
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
                    res.AssertOk();
                    var body = await res.ReadBodyAsJson<UserInfo>();
                    Assert.Equal(administrator, body.Administrator);
                }

                {
                    // Put Created.
                    var res = await client.PutAsJsonAsync(url, new UserPutRequest
                    {
                        Password = password,
                        Administrator = false
                    });
                    await res.AssertIsPutCreated();
                    await CheckAdministrator(false);
                }

                {
                    // Put Modified.
                    var res = await client.PutAsJsonAsync(url, new UserPutRequest
                    {
                        Password = password,
                        Administrator = true
                    });
                    await res.AssertIsPutModified();
                    await CheckAdministrator(true);
                }

                // Patch Not Exist
                {
                    var res = await client.PatchAsJsonAsync("users/usernotexist", new UserPatchRequest { });
                    res.AssertNotFound();
                    var body = await res.ReadBodyAsJson<CommonResponse>();
                    Assert.Equal(UserController.ErrorCodes.Patch_NotExist, body.Code);
                }

                // Patch Success
                {
                    var res = await client.PatchAsJsonAsync(url, new UserPatchRequest { Administrator = false });
                    res.AssertOk();
                    await CheckAdministrator(false);
                }

                // Delete Deleted
                {
                    var res = await client.DeleteAsync(url);
                    await res.AssertIsDeleteDeleted();

                    var res2 = await client.GetAsync(url);
                    res2.AssertNotFound();
                }

                // Delete Not Exist
                {
                    var res = await client.DeleteAsync(url);
                    await res.AssertIsDeleteNotExist();
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
                    res.AssertBadRequest();
                    var body = await res.ReadBodyAsJson<CommonResponse>();
                    Assert.Equal(UserController.ErrorCodes.ChangePassword_BadOldPassword, body.Code);
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
                    Assert.Equal(HttpStatusCode.Created, res.StatusCode);
                }

                using (var client = await _factory.CreateClientWithCredential(username, password))
                {
                    const string newPassword = "new";
                    var res = await client.PostAsJsonAsync(url, new ChangePasswordRequest { OldPassword = password, NewPassword = newPassword });
                    res.AssertOk();
                    await client.CreateUserTokenAsync(username, newPassword);
                }
            }
        }
    }
}
