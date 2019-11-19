using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Timeline.Tests.Helpers;
using Xunit;

namespace Timeline.Tests.IntegratedTests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:Uri parameters should not be strings")]
    public class I18nTest : IntegratedTestBase
    {
        private readonly HttpClient _client;

        public I18nTest(WebApplicationFactory<Startup> factory)
            : base(factory)
        {
            _client = Factory.CreateDefaultClient();
        }

        protected override void OnDispose()
        {
            _client.Dispose();
        }

        private const string DirectUrl = "testing/i18n/direct";
        private const string LocalizerUrl = "testing/i18n/localizer";

        [Theory]
        [InlineData(DirectUrl)]
        [InlineData(LocalizerUrl)]
        public async Task DefaultShouldReturnEnglish(string url)
        {
            (await _client.GetStringAsync(url)).Should().ContainEquivalentOf("English");
        }

        [Theory]
        [InlineData(DirectUrl, "en", true)]
        [InlineData(LocalizerUrl, "en", true)]
        [InlineData(DirectUrl, "en-US", true)]
        [InlineData(LocalizerUrl, "en-US", true)]
        [InlineData(DirectUrl, "zh", false)]
        [InlineData(LocalizerUrl, "zh", false)]
        public async Task ShouldWork(string url, string acceptLanguage, bool english)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url, UriKind.RelativeOrAbsolute)
            };
            request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue(acceptLanguage));
            var body = await (await _client.SendAsync(request)).Content.ReadAsStringAsync();
            body.Should().ContainEquivalentOf(english ? "English" : "中文");
            request.Dispose();
        }
    }
}
