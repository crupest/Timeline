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
            var res = await client.GetAsync("unknownEndpoint");
            res.Should().HaveStatusCode(400)
                .And.HaveCommonBody()
                .Which.Code.Should().Be(ErrorCodes.Common.UnknownEndpoint);
        }
    }
}
