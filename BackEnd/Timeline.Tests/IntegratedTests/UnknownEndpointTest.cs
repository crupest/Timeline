using FluentAssertions;
using System.Threading.Tasks;
using Timeline.Models.Http;
using Timeline.Tests.Helpers;
using Xunit;

namespace Timeline.Tests.IntegratedTests
{
    public class UnknownEndpointTest : IntegratedTestBase
    {
        [Fact]
        public async Task UnknownEndpoint()
        {
            using var client = await CreateDefaultClient();
            await client.TestGetAssertErrorAsync("unknownEndpoint", errorCode: ErrorCodes.Common.UnknownEndpoint);
        }
    }
}
