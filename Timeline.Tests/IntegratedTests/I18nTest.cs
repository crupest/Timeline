using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Timeline.Tests.Helpers;
using Xunit;
using FluentAssertions;

namespace Timeline.Tests.IntegratedTests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:Uri parameters should not be strings")]
    public class I18nTest : IClassFixture<WebApplicationFactory<Startup>>, IDisposable
    {
        private readonly TestApplication _testApp;
        private readonly HttpClient _client;

        public I18nTest(WebApplicationFactory<Startup> factory)
        {
            _testApp = new TestApplication(factory);
            _client = _testApp.Factory.CreateDefaultClient();
        }

        public void Dispose()
        {
            _client.Dispose();
            _testApp.Dispose();
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
