using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Timeline.Services;
using Timeline.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests
{
    public class QCloudCosServiceUnitTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public QCloudCosServiceUnitTest(WebApplicationFactory<Startup> factory, ITestOutputHelper outputHelper)
        {
            _factory = factory.WithTestConfig(outputHelper);
            _factory.CreateDefaultClient().Dispose(); // Ensure test server is created.
        }

        [Fact]
        public void ValidateBucketNameTest()
        {
            Assert.True(QCloudCosService.ValidateBucketName("hello"));
            Assert.True(QCloudCosService.ValidateBucketName("hello0123"));
            Assert.True(QCloudCosService.ValidateBucketName("hello0123-hello"));
            Assert.False(QCloudCosService.ValidateBucketName("-hello"));
            Assert.False(QCloudCosService.ValidateBucketName("hello-"));
            Assert.False(QCloudCosService.ValidateBucketName("helloU"));
            Assert.False(QCloudCosService.ValidateBucketName("hello!"));
        }

        [Fact]
        public void GenerateSignatureTest()
        {
            var credential = new QCloudCosService.QCloudCredentials
            {
                SecretId = "AKIDQjz3ltompVjBni5LitkWHFlFpwkn9U5q",
                SecretKey = "BQYIM75p8x0iWVFSIgqEKwFprpRSVHlz"
            };

            var request = new QCloudCosService.RequestInfo
            {
                Method = "put",
                Uri = "/exampleobject",
                Parameters = new Dictionary<string, string>(),
                Headers = new Dictionary<string, string>
                {
                    ["Host"] = "examplebucket-1250000000.cos.ap-beijing.myqcloud.com",
                    ["x-cos-storage-class"] = "standard",
                    ["x-cos-content-sha1"] = "b502c3a1f48c8609ae212cdfb639dee39673f5e"
                }
            };

            var signValidTime = new QCloudCosService.TimeDuration
            {
                Start = DateTimeOffset.FromUnixTimeSeconds(1417773892),
                End = DateTimeOffset.FromUnixTimeSeconds(1417853898)
            };

            Assert.Equal("q-sign-algorithm=sha1&q-ak=AKIDQjz3ltompVjBni5LitkWHFlFpwkn9U5q&q-sign-time=1417773892;1417853898&q-key-time=1417773892;1417853898&q-header-list=host;x-cos-content-sha1;x-cos-storage-class&q-url-param-list=&q-signature=0ab12f43e74cbe148d705cd9fae8adc9a6d39cc1", QCloudCosService.GenerateSign(credential, request, signValidTime));
        }

/*
// Tests in this part need secret configs in cos.
#region SecretTests
        [Fact]
        public async Task ObjectExistsTest()
        {
            using (var serviceScope = _factory.Server.Host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                var service = services.GetRequiredService<IQCloudCosService>();
                Assert.True(await service.IsObjectExists("avatar", "__default"));
                Assert.False(await service.IsObjectExists("avatar", "haha"));
                Assert.False(await service.IsObjectExists("haha", "haha"));                
            }
        }

        // Although this test does not pass on my archlunux system. But the GenerateObjectGetUrl actually works well.
        // And I don't know why.
        [Fact]
        public async Task GenerateObjectGetUrlTest()
        {
            using (var serviceScope = _factory.Server.Host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                var service = services.GetRequiredService<IQCloudCosService>();
                var url = service.GenerateObjectGetUrl("avatar", "__default");
                using (var client = _factory.CreateClient())
                {
                    var res = await client.GetAsync(url);
                    Assert.Equal(HttpStatusCode.OK, res.StatusCode);
                }
            }
        }
#endregion
*/
    }
}
