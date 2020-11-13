using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Timeline.Models.Http;
using Timeline.Services;
using Xunit;

namespace Timeline.Tests.IntegratedTests
{
    public class UserPermissionTest : IntegratedTestBase
    {
        public UserPermissionTest() : base(3) { }

        [Fact]
        public async Task RootUserShouldReturnAllPermissions()
        {
            using var client = await CreateDefaultClient();
            var res = await client.GetAsync("users/admin");
            res.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await res.Content.ReadFromJsonAsync<UserInfo>();
            body.Permissions.Should().BeEquivalentTo(Enum.GetNames<UserPermission>());
        }

        [Fact]
        public async Task NonRootUserShouldReturnNonPermissions()
        {
            using var client = await CreateDefaultClient();
            var res = await client.GetAsync("users/user1");
            res.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await res.Content.ReadFromJsonAsync<UserInfo>();
            body.Permissions.Should().BeEmpty();
        }

        public static IEnumerable<object[]> EveryPermissionTestData()
        {
            return Enum.GetValues<UserPermission>().Select(p => new object[] { p });
        }

        [Theory]
        [MemberData(nameof(EveryPermissionTestData))]
        public async Task ModifyRootUserPermissionShouldHaveNoEffect(UserPermission permission)
        {
            using var client = await CreateClientAsAdministrator();

            {
                var res = await client.DeleteAsync($"users/admin/permissions/{permission}");
                res.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            {
                var res = await client.GetAsync("users/admin");
                res.StatusCode.Should().Be(HttpStatusCode.OK);
                var body = await res.Content.ReadFromJsonAsync<UserInfo>();
                body.Permissions.Should().BeEquivalentTo(Enum.GetNames<UserPermission>());
            }

            {
                var res = await client.PutAsync($"users/admin/permissions/{permission}", null);
                res.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            {
                var res = await client.GetAsync("users/admin");
                res.StatusCode.Should().Be(HttpStatusCode.OK);
                var body = await res.Content.ReadFromJsonAsync<UserInfo>();
                body.Permissions.Should().BeEquivalentTo(Enum.GetNames<UserPermission>());
            }
        }

        [Theory]
        [MemberData(nameof(EveryPermissionTestData))]
        public async Task ModifyUserPermissionShouldWork(UserPermission permission)
        {
            using var client = await CreateClientAsAdministrator();

            {
                var res = await client.PutAsync($"users/user1/permissions/{permission}", null);
                res.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            {
                var res = await client.GetAsync("users/user1");
                res.StatusCode.Should().Be(HttpStatusCode.OK);
                var body = await res.Content.ReadFromJsonAsync<UserInfo>();
                body.Permissions.Should().BeEquivalentTo(permission.ToString());
            }

            {
                var res = await client.DeleteAsync($"users/user1/permissions/{permission}");
                res.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            {
                var res = await client.GetAsync("users/user1");
                res.StatusCode.Should().Be(HttpStatusCode.OK);
                var body = await res.Content.ReadFromJsonAsync<UserInfo>();
                body.Permissions.Should().BeEmpty();
            }
        }

        [Theory]
        [MemberData(nameof(EveryPermissionTestData))]
        public async Task PutExistPermissionShouldHaveNoEffect(UserPermission permission)
        {
            using var client = await CreateClientAsAdministrator();

            {
                var res = await client.PutAsync($"users/user1/permissions/{permission}", null);
                res.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            {
                var res = await client.GetAsync("users/user1");
                res.StatusCode.Should().Be(HttpStatusCode.OK);
                var body = await res.Content.ReadFromJsonAsync<UserInfo>();
                body.Permissions.Should().BeEquivalentTo(permission.ToString());
            }

            {
                var res = await client.PutAsync($"users/user1/permissions/{permission}", null);
                res.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            {
                var res = await client.GetAsync("users/user1");
                res.StatusCode.Should().Be(HttpStatusCode.OK);
                var body = await res.Content.ReadFromJsonAsync<UserInfo>();
                body.Permissions.Should().BeEquivalentTo(permission.ToString());
            }
        }

        [Theory]
        [MemberData(nameof(EveryPermissionTestData))]
        public async Task DeleteNonExistPermissionShouldHaveNoEffect(UserPermission permission)
        {
            using var client = await CreateClientAsAdministrator();

            {
                var res = await client.DeleteAsync($"users/user1/permissions/{permission}");
                res.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            {
                var res = await client.GetAsync("users/user1");
                res.StatusCode.Should().Be(HttpStatusCode.OK);
                var body = await res.Content.ReadFromJsonAsync<UserInfo>();
                body.Permissions.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task AGeneralTest()
        {
            using var client = await CreateClientAsAdministrator();

            {
                var res = await client.PutAsync($"users/user1/permissions/{UserPermission.AllTimelineManagement}", null);
                res.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            {
                var res = await client.GetAsync("users/user1");
                res.StatusCode.Should().Be(HttpStatusCode.OK);
                var body = await res.Content.ReadFromJsonAsync<UserInfo>();
                body.Permissions.Should().BeEquivalentTo(UserPermission.AllTimelineManagement.ToString());
            }

            {
                var res = await client.PutAsync($"users/user1/permissions/{UserPermission.HighlightTimelineManangement}", null);
                res.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            {
                var res = await client.GetAsync("users/user1");
                res.StatusCode.Should().Be(HttpStatusCode.OK);
                var body = await res.Content.ReadFromJsonAsync<UserInfo>();
                body.Permissions.Should().BeEquivalentTo(UserPermission.AllTimelineManagement.ToString(),
                    UserPermission.HighlightTimelineManangement.ToString());
            }

            {
                var res = await client.PutAsync($"users/user1/permissions/{UserPermission.UserManagement}", null);
                res.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            {
                var res = await client.GetAsync("users/user1");
                res.StatusCode.Should().Be(HttpStatusCode.OK);
                var body = await res.Content.ReadFromJsonAsync<UserInfo>();
                body.Permissions.Should().BeEquivalentTo(
                    UserPermission.AllTimelineManagement.ToString(),
                    UserPermission.HighlightTimelineManangement.ToString(),
                    UserPermission.UserManagement.ToString());
            }

            {
                var res = await client.DeleteAsync($"users/user1/permissions/{UserPermission.HighlightTimelineManangement}");
                res.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            {
                var res = await client.GetAsync("users/user1");
                res.StatusCode.Should().Be(HttpStatusCode.OK);
                var body = await res.Content.ReadFromJsonAsync<UserInfo>();
                body.Permissions.Should().BeEquivalentTo(
                    UserPermission.AllTimelineManagement.ToString(),
                    UserPermission.UserManagement.ToString());
            }

            {
                var res = await client.DeleteAsync($"users/user1/permissions/{UserPermission.AllTimelineManagement}");
                res.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            {
                var res = await client.GetAsync("users/user1");
                res.StatusCode.Should().Be(HttpStatusCode.OK);
                var body = await res.Content.ReadFromJsonAsync<UserInfo>();
                body.Permissions.Should().BeEquivalentTo(UserPermission.UserManagement.ToString());
            }

            {
                var res = await client.PutAsync($"users/user1/permissions/{UserPermission.HighlightTimelineManangement}", null);
                res.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            {
                var res = await client.GetAsync("users/user1");
                res.StatusCode.Should().Be(HttpStatusCode.OK);
                var body = await res.Content.ReadFromJsonAsync<UserInfo>();
                body.Permissions.Should().BeEquivalentTo(
                    UserPermission.HighlightTimelineManangement.ToString(), UserPermission.UserManagement.ToString());
            }

            {
                var res = await client.DeleteAsync($"users/user1/permissions/{UserPermission.HighlightTimelineManangement}");
                res.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            {
                var res = await client.GetAsync("users/user1");
                res.StatusCode.Should().Be(HttpStatusCode.OK);
                var body = await res.Content.ReadFromJsonAsync<UserInfo>();
                body.Permissions.Should().BeEquivalentTo(UserPermission.UserManagement.ToString());
            }

            {
                var res = await client.DeleteAsync($"users/user1/permissions/{UserPermission.UserManagement}");
                res.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            {
                var res = await client.GetAsync("users/user1");
                res.StatusCode.Should().Be(HttpStatusCode.OK);
                var body = await res.Content.ReadFromJsonAsync<UserInfo>();
                body.Permissions.Should().BeEmpty();
            }
        }

        [Theory]
        [InlineData("users/user1/permissions/aaa")]
        [InlineData("users/!!!/permissions/UserManagement")]
        public async Task InvalidModel(string url)
        {
            using var client = await CreateClientAsAdministrator();

            {
                var res = await client.PutAsync(url, null);
                res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var body = await res.Content.ReadFromJsonAsync<CommonResponse>();
                body.Code.Should().Be(ErrorCodes.Common.InvalidModel);
            }

            {
                var res = await client.DeleteAsync(url);
                res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var body = await res.Content.ReadFromJsonAsync<CommonResponse>();
                body.Code.Should().Be(ErrorCodes.Common.InvalidModel);
            }
        }

        [Fact]
        public async Task UserNotExist()
        {
            using var client = await CreateClientAsAdministrator();

            const string url = "users/user123/permissions/UserManagement";

            {
                var res = await client.PutAsync(url, null);
                res.StatusCode.Should().Be(HttpStatusCode.NotFound);
                var body = await res.Content.ReadFromJsonAsync<CommonResponse>();
                body.Code.Should().Be(ErrorCodes.UserCommon.NotExist);
            }

            {
                var res = await client.DeleteAsync(url);
                res.StatusCode.Should().Be(HttpStatusCode.NotFound);
                var body = await res.Content.ReadFromJsonAsync<CommonResponse>();
                body.Code.Should().Be(ErrorCodes.UserCommon.NotExist);
            }
        }
    }
}
