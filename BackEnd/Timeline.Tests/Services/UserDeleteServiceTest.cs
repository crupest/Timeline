using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Threading.Tasks;
using Timeline.Services;
using Timeline.Services.Exceptions;
using Xunit;

namespace Timeline.Tests.Services
{
    public class UserDeleteServiceTest : DatabaseBasedTest
    {
        private readonly Mock<ITimelineService> _mockTimelineService = new Mock<ITimelineService>();
        private UserDeleteService _service = default!;

        protected override void OnDatabaseCreated()
        {
            _service = new UserDeleteService(NullLogger<UserDeleteService>.Instance, Database, _mockTimelineService.Object);
        }

        [Fact]
        public async Task DeleteRootUser_Should_Throw()
        {
            await _service.Awaiting(s => s.DeleteUser("admin")).Should().ThrowAsync<InvalidOperationOnRootUserException>();
        }
    }
}
