﻿using System;
using System.Collections.Generic;
using Timeline.Services;
using Xunit;

namespace Timeline.Tests
{
    public class QCloudCosServiceUnitTest
    {
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
    }
}
