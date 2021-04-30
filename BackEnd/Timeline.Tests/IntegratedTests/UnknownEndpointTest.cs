using System.Threading.Tasks;
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
