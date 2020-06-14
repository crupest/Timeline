using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Mime;
using System.Threading.Tasks;
using Timeline.Tests.Helpers;
using Xunit;

namespace Timeline.Tests.IntegratedTests
{
    public class FrontEndTest : IntegratedTestBase
    {
        public FrontEndTest(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        [Fact]
        public async Task Index()
        {
            using var client = await CreateDefaultClient(false);
            var res = await client.GetAsync("index.html");
            res.Should().HaveStatusCode(200);
            res.Content.Headers.ContentType.MediaType.Should().Be(MediaTypeNames.Text.Html);
        }

        [Fact]
        public async Task Fallback()
        {
            using var client = await CreateDefaultClient(false);
            var res = await client.GetAsync("aaaaaaaaaaaaaaa");
            res.Should().HaveStatusCode(200);
            res.Content.Headers.ContentType.MediaType.Should().Be(MediaTypeNames.Text.Html);
        }
    }
}
