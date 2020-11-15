using FluentAssertions;
using System;
using System.Threading.Tasks;
using Timeline.Services;
using Timeline.Services.Exceptions;
using Xunit;

namespace Timeline.Tests.Services
{
    public class UserPermissionServiceTest : DatabaseBasedTest
    {
        private UserPermissionService _service = default!;

        public UserPermissionServiceTest()
        {

        }

        protected override void OnDatabaseCreated()
        {
            _service = new UserPermissionService(Database);
        }

        [Fact]
        public async Task GetPermissionsOfRootUserShouldReturnAll()
        {
            var permission = await _service.GetPermissionsOfUserAsync(1);
            permission.Should().BeEquivalentTo(Enum.GetValues<UserPermission>());
        }

        [Fact]
        public async Task GetPermissionsOfNonRootUserShouldReturnNone()
        {
            var permission = await _service.GetPermissionsOfUserAsync(2);
            permission.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPermissionsOfInexistentUserShouldThrow()
        {
            await _service.Awaiting(s => s.GetPermissionsOfUserAsync(10)).Should().ThrowAsync<UserNotExistException>();
        }

        [Fact]
        public async Task GetPermissionsOfInexistentUserShouldNotThrowIfNotCheck()
        {
            await _service.Awaiting(s => s.GetPermissionsOfUserAsync(10, false)).Should().NotThrowAsync();
        }

        [Fact]
        public async Task ModifyPermissionOnRootUserShouldHaveNoEffect()
        {
            await _service.AddPermissionToUserAsync(1, UserPermission.AllTimelineManagement);
            {
                var permission = await _service.GetPermissionsOfUserAsync(1);
                permission.Should().BeEquivalentTo(Enum.GetValues<UserPermission>());
            }
            await _service.RemovePermissionFromUserAsync(1, UserPermission.AllTimelineManagement);
            {
                var permission = await _service.GetPermissionsOfUserAsync(1);
                permission.Should().BeEquivalentTo(Enum.GetValues<UserPermission>());
            }
        }

        [Fact]
        public async Task ModifyPermissionOnNonRootUserShouldWork()
        {
            await _service.AddPermissionToUserAsync(2, UserPermission.AllTimelineManagement);
            {
                var permission = await _service.GetPermissionsOfUserAsync(2);
                permission.Should().BeEquivalentTo(UserPermission.AllTimelineManagement);
            }
            await _service.AddPermissionToUserAsync(2, UserPermission.HighlightTimelineManangement);
            {
                var permission = await _service.GetPermissionsOfUserAsync(2);
                permission.Should().BeEquivalentTo(UserPermission.AllTimelineManagement, UserPermission.HighlightTimelineManangement);
            }

            // Add duplicate permission should work.
            await _service.AddPermissionToUserAsync(2, UserPermission.HighlightTimelineManangement);
            {
                var permission = await _service.GetPermissionsOfUserAsync(2);
                permission.Should().BeEquivalentTo(UserPermission.AllTimelineManagement, UserPermission.HighlightTimelineManangement);
            }

            await _service.RemovePermissionFromUserAsync(2, UserPermission.HighlightTimelineManangement);
            {
                var permission = await _service.GetPermissionsOfUserAsync(2);
                permission.Should().BeEquivalentTo(UserPermission.AllTimelineManagement);
            }

            // Remove non-owned permission should work.
            await _service.RemovePermissionFromUserAsync(2, UserPermission.HighlightTimelineManangement);
            {
                var permission = await _service.GetPermissionsOfUserAsync(2);
                permission.Should().BeEquivalentTo(UserPermission.AllTimelineManagement);
            }
        }

        [Fact]
        public async Task AddPermissionToInexistentUserShouldThrown()
        {
            await _service.Awaiting(s => s.AddPermissionToUserAsync(10, UserPermission.HighlightTimelineManangement)).Should().ThrowAsync<UserNotExistException>();
        }

        [Fact]
        public async Task RemovePermissionFromInexistentUserShouldThrown()
        {
            await _service.Awaiting(s => s.RemovePermissionFromUserAsync(10, UserPermission.HighlightTimelineManangement)).Should().ThrowAsync<UserNotExistException>();
        }

        [Fact]
        public async Task RemovePermissionFromInexistentUserShouldNotThrownIfNotCheck()
        {
            await _service.Awaiting(s => s.RemovePermissionFromUserAsync(10, UserPermission.HighlightTimelineManangement, false)).Should().NotThrowAsync();
        }
    }
}
