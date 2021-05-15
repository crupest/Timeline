using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests.IntegratedTests
{
    public class UnknownEndpointTest : IntegratedTestBase
    {
        public UnknownEndpointTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {

        }

        [Fact]
        public async Task UnknownEndpoint()
        {
            using var client = await CreateDefaultClient();
            await client.TestGetAssertErrorAsync("unknownEndpoint", errorCode: ErrorCodes.Common.UnknownEndpoint);
        }
    }
}
