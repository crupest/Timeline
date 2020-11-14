using FluentAssertions;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
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
            res.StatusCode.Should().Be(HttpStatusCode.OK);
            var contentTypeHeader = res.Content.Headers.ContentType;
            contentTypeHeader.Should().NotBeNull();
            contentTypeHeader!.MediaType.Should().Be(MediaTypeNames.Text.Html);
        }

        [Fact]
        public async Task Fallback()
        {
            using var client = await CreateDefaultClient(false);
            var res = await client.GetAsync("aaaaaaaaaaaaaaa");
            res.StatusCode.Should().Be(HttpStatusCode.OK);
            var contentTypeHeader = res.Content.Headers.ContentType;
            contentTypeHeader.Should().NotBeNull();
            contentTypeHeader!.MediaType.Should().Be(MediaTypeNames.Text.Html);
        }
    }
}
