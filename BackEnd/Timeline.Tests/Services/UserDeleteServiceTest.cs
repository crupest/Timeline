using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Threading.Tasks;
using Timeline.Services;
using Timeline.Services.Exceptions;
using Xunit;

namespace Timeline.Tests.Services
{
    public class UserDeleteServiceTest : ServiceTestBase
    {
        private readonly Mock<ITimelinePostService> _mockTimelinePostService = new Mock<ITimelinePostService>();
        private UserDeleteService _service = default!;

        protected override void OnInitialize()
        {
            _service = new UserDeleteService(NullLogger<UserDeleteService>.Instance, Database, _mockTimelinePostService.Object);
        }

        [Fact]
        public async Task DeleteRootUser_Should_Throw()
        {
            await _service.Awaiting(s => s.DeleteUser("admin")).Should().ThrowAsync<InvalidOperationOnRootUserException>();
        }
    }
}
