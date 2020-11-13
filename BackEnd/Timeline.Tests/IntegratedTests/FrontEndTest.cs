using FluentAssertions;
using System.Net.Mime;
using System.Threading.Tasks;
using Timeline.Tests.Helpers;
using Xunit;

namespace Timeline.Tests.IntegratedTests
{
    public class FrontEndTest : IntegratedTestBase
    {
        [Fact]
        public async Task Index()
        {
            using var client = await CreateDefaultClient(false);
            var res = await client.GetAsync("index.html");
            res.Should().HaveStatusCode(200);
            var contentTypeHeader = res.Content.Headers.ContentType;
            contentTypeHeader.Should().NotBeNull();
            contentTypeHeader!.MediaType.Should().Be(MediaTypeNames.Text.Html);
        }

        [Fact]
        public async Task Fallback()
        {
            using var client = await CreateDefaultClient(false);
            var res = await client.GetAsync("aaaaaaaaaaaaaaa");
            res.Should().HaveStatusCode(200);
            var contentTypeHeader = res.Content.Headers.ContentType;
            contentTypeHeader.Should().NotBeNull();
            contentTypeHeader!.MediaType.Should().Be(MediaTypeNames.Text.Html);
        }
    }
}
