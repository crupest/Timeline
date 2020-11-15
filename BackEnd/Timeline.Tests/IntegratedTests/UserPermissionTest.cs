using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var body = await client.GetUserAsync("admin");
            body.Permissions.Should().BeEquivalentTo(Enum.GetNames<UserPermission>());
        }

        [Fact]
        public async Task NonRootUserShouldReturnNonPermissions()
        {
            using var client = await CreateDefaultClient();
            var body = await client.GetUserAsync("user1");
            body.Permissions.Should().BeEmpty();
        }

        public static IEnumerable<object[]> EveryPermissionTestData()
        {
            return Enum.GetValues<UserPermission>().Select(p => new object[] { p });
        }

        [Theory]
        [MemberData(nameof(EveryPermissionTestData))]
        public async Task ModifyRootUserPermission_Should_Error(UserPermission permission)
        {
            using var client = await CreateClientAsAdministrator();

            await client.TestPutAssertErrorAsync($"users/admin/permissions/{permission}",
                errorCode: ErrorCodes.UserController.ChangePermission_RootUser);

            await client.TestDeleteAssertErrorAsync($"users/admin/permissions/{permission}",
                errorCode: ErrorCodes.UserController.ChangePermission_RootUser);
        }

        [Theory]
        [MemberData(nameof(EveryPermissionTestData))]
        public async Task ModifyUserPermissionShouldWork(UserPermission permission)
        {
            using var client = await CreateClientAsAdministrator();

            await client.TestPutAsync($"users/user1/permissions/{permission}");

            {
                var body = await client.GetUserAsync("user1");
                body.Permissions.Should().BeEquivalentTo(permission.ToString());
            }

            await client.TestDeleteAsync($"users/user1/permissions/{permission}");

            {
                var body = await client.GetUserAsync("user1");
                body.Permissions.Should().BeEmpty();
            }
        }

        [Theory]
        [MemberData(nameof(EveryPermissionTestData))]
        public async Task PutExistPermissionShouldHaveNoEffect(UserPermission permission)
        {
            using var client = await CreateClientAsAdministrator();

            await client.TestPutAsync($"users/user1/permissions/{permission}");

            {
                var body = await client.GetUserAsync("user1");
                body.Permissions.Should().BeEquivalentTo(permission.ToString());
            }

            await client.TestPutAsync($"users/user1/permissions/{permission}");

            {
                var body = await client.GetUserAsync("user1");
                body.Permissions.Should().BeEquivalentTo(permission.ToString());
            }
        }

        [Theory]
        [MemberData(nameof(EveryPermissionTestData))]
        public async Task DeleteNonExistPermissionShouldHaveNoEffect(UserPermission permission)
        {
            using var client = await CreateClientAsAdministrator();

            await client.TestDeleteAsync($"users/user1/permissions/{permission}");

            {
                var body = await client.GetUserAsync("user1");
                body.Permissions.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task AGeneralTest()
        {
            using var client = await CreateClientAsAdministrator();

            await client.TestPutAsync($"users/user1/permissions/{UserPermission.AllTimelineManagement}");

            {
                var body = await client.GetUserAsync("user1");
                body.Permissions.Should().BeEquivalentTo(UserPermission.AllTimelineManagement.ToString());
            }

            await client.TestPutAsync($"users/user1/permissions/{UserPermission.HighlightTimelineManangement}");

            {
                var body = await client.GetUserAsync("user1");
                body.Permissions.Should().BeEquivalentTo(UserPermission.AllTimelineManagement.ToString(),
                    UserPermission.HighlightTimelineManangement.ToString());
            }

            await client.TestPutAsync($"users/user1/permissions/{UserPermission.UserManagement}");

            {
                var body = await client.GetUserAsync("user1");
                body.Permissions.Should().BeEquivalentTo(
                    UserPermission.AllTimelineManagement.ToString(),
                    UserPermission.HighlightTimelineManangement.ToString(),
                    UserPermission.UserManagement.ToString());
            }

            await client.TestDeleteAsync($"users/user1/permissions/{UserPermission.HighlightTimelineManangement}");

            {
                var body = await client.GetUserAsync("user1");
                body.Permissions.Should().BeEquivalentTo(
                    UserPermission.AllTimelineManagement.ToString(),
                    UserPermission.UserManagement.ToString());
            }

            await client.TestDeleteAsync($"users/user1/permissions/{UserPermission.AllTimelineManagement}");

            {
                var body = await client.GetUserAsync("user1");
                body.Permissions.Should().BeEquivalentTo(UserPermission.UserManagement.ToString());
            }

            await client.TestPutAsync($"users/user1/permissions/{UserPermission.HighlightTimelineManangement}");

            {
                var body = await client.GetUserAsync("user1");
                body.Permissions.Should().BeEquivalentTo(
                    UserPermission.HighlightTimelineManangement.ToString(), UserPermission.UserManagement.ToString());
            }

            await client.TestDeleteAsync($"users/user1/permissions/{UserPermission.HighlightTimelineManangement}");

            {
                var body = await client.GetUserAsync("user1");
                body.Permissions.Should().BeEquivalentTo(UserPermission.UserManagement.ToString());
            }

            await client.TestDeleteAsync($"users/user1/permissions/{UserPermission.UserManagement}");

            {
                var body = await client.GetUserAsync("user1");
                body.Permissions.Should().BeEmpty();
            }
        }

        [Theory]
        [InlineData("users/user1/permissions/aaa")]
        [InlineData("users/!!!/permissions/UserManagement")]
        public async Task InvalidModel(string url)
        {
            using var client = await CreateClientAsAdministrator();

            await client.TestPutAssertInvalidModelAsync(url);
            await client.TestDeleteAssertInvalidModelAsync(url);
        }

        [Fact]
        public async Task UserNotExist()
        {
            using var client = await CreateClientAsAdministrator();

            const string url = "users/user123/permissions/UserManagement";

            await client.TestPutAssertNotFoundAsync(url, errorCode: ErrorCodes.UserCommon.NotExist);
            await client.TestDeleteAssertNotFoundAsync(url, errorCode: ErrorCodes.UserCommon.NotExist);
        }
    }
}
