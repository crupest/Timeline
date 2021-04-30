using FluentAssertions;
using System;
using System.Threading.Tasks;
using Timeline.Services;
using Timeline.Services.User;
using Xunit;

namespace Timeline.Tests.Services
{
    public class UserPermissionServiceTest : ServiceTestBase
    {
        private UserPermissionService _service = default!;

        public UserPermissionServiceTest()
        {

        }

        protected override void OnInitialize()
        {
            _service = new UserPermissionService(Database, UserService);
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
            await _service.Awaiting(s => s.GetPermissionsOfUserAsync(10)).Should().ThrowAsync<EntityNotExistException>();
        }

        [Fact]
        public async Task GetPermissionsOfInexistentUserShouldNotThrowIfNotCheck()
        {
            await _service.Awaiting(s => s.GetPermissionsOfUserAsync(10, false)).Should().NotThrowAsync();
        }

        [Fact]
        public async Task ModifyPermissionOnRootUser_Should_Throw()
        {
            await _service.Awaiting(s => s.AddPermissionToUserAsync(1, UserPermission.AllTimelineManagement)).Should().ThrowAsync<InvalidOperationOnRootUserException>();
            await _service.Awaiting(s => s.RemovePermissionFromUserAsync(1, UserPermission.AllTimelineManagement)).Should().ThrowAsync<InvalidOperationOnRootUserException>();
        }

        [Fact]
        public async Task ModifyPermissionOnNonRootUserShouldWork()
        {
            await _service.AddPermissionToUserAsync(2, UserPermission.AllTimelineManagement);
            {
                var permission = await _service.GetPermissionsOfUserAsync(2);
                permission.Should().BeEquivalentTo(UserPermission.AllTimelineManagement);
            }
            await _service.AddPermissionToUserAsync(2, UserPermission.HighlightTimelineManagement);
            {
                var permission = await _service.GetPermissionsOfUserAsync(2);
                permission.Should().BeEquivalentTo(UserPermission.AllTimelineManagement, UserPermission.HighlightTimelineManagement);
            }

            // Add duplicate permission should work.
            await _service.AddPermissionToUserAsync(2, UserPermission.HighlightTimelineManagement);
            {
                var permission = await _service.GetPermissionsOfUserAsync(2);
                permission.Should().BeEquivalentTo(UserPermission.AllTimelineManagement, UserPermission.HighlightTimelineManagement);
            }

            await _service.RemovePermissionFromUserAsync(2, UserPermission.HighlightTimelineManagement);
            {
                var permission = await _service.GetPermissionsOfUserAsync(2);
                permission.Should().BeEquivalentTo(UserPermission.AllTimelineManagement);
            }

            // Remove non-owned permission should work.
            await _service.RemovePermissionFromUserAsync(2, UserPermission.HighlightTimelineManagement);
            {
                var permission = await _service.GetPermissionsOfUserAsync(2);
                permission.Should().BeEquivalentTo(UserPermission.AllTimelineManagement);
            }
        }

        [Fact]
        public async Task AddPermissionToInexistentUserShouldThrown()
        {
            await _service.Awaiting(s => s.AddPermissionToUserAsync(10, UserPermission.HighlightTimelineManagement)).Should().ThrowAsync<EntityNotExistException>();
        }

        [Fact]
        public async Task RemovePermissionFromInexistentUserShouldThrown()
        {
            await _service.Awaiting(s => s.RemovePermissionFromUserAsync(10, UserPermission.HighlightTimelineManagement)).Should().ThrowAsync<EntityNotExistException>();
        }

        [Fact]
        public async Task RemovePermissionFromInexistentUserShouldNotThrownIfNotCheck()
        {
            await _service.Awaiting(s => s.RemovePermissionFromUserAsync(10, UserPermission.HighlightTimelineManagement, false)).Should().NotThrowAsync();
        }
    }
}
