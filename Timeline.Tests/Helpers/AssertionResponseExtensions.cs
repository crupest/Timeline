using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using Timeline.Models.Http;

namespace Timeline.Tests.Helpers
{
    public class HttpResponseMessageAssertions
        : ReferenceTypeAssertions<HttpResponseMessage, HttpResponseMessageAssertions>
    {
        public HttpResponseMessageAssertions(HttpResponseMessage instance)
        {
            Subject = instance;
        }

        protected override string Identifier => "HttpResponseMessage";

        public AndConstraint<HttpResponseMessage> HaveStatusCode(HttpStatusCode expected, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion.BecauseOf(because, becauseArgs)
                .ForCondition(Subject.StatusCode == expected)
                .FailWith("Expected status code is {}, but found {}.", expected, Subject.StatusCode);
            return new AndConstraint<HttpResponseMessage>(Subject);
        }

        public AndWhichConstraint<HttpResponseMessage, T> HaveBodyAsJson<T>(string because = "", params object[] becauseArgs)
        {
            var a = Execute.Assertion.BecauseOf(because, becauseArgs);
            string body;
            try
            {
                body = Subject.Content.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                a.FailWith("Failed to read response body. Exception is {}.", e);
                return new AndWhichConstraint<HttpResponseMessage, T>(Subject, null);
            }

            try
            {
                var result = JsonConvert.DeserializeObject<T>(body);
                return new AndWhichConstraint<HttpResponseMessage, T>(Subject, result);
            }
            catch (Exception e)
            {
                a.FailWith("Failed to convert response body to {}. Exception is {}.", typeof(T).FullName, e);
                return new AndWhichConstraint<HttpResponseMessage, T>(Subject, null);
            }
        }
    }

    public static class AssertionResponseExtensions
    {
        public static HttpResponseMessageAssertions Should(this HttpResponseMessage instance)
        {
            return new HttpResponseMessageAssertions(instance);
        }

        public static AndConstraint<HttpResponseMessage> HaveStatusCodeOk(this HttpResponseMessageAssertions assertions, string because = "", params object[] becauseArgs)
        {
            return assertions.HaveStatusCode(HttpStatusCode.OK, because, becauseArgs);
        }

        public static AndConstraint<HttpResponseMessage> HaveStatusCodeCreated(this HttpResponseMessageAssertions assertions, string because = "", params object[] becauseArgs)
        {
            return assertions.HaveStatusCode(HttpStatusCode.Created, because, becauseArgs);
        }

        public static AndConstraint<HttpResponseMessage> HaveStatusCodeBadRequest(this HttpResponseMessageAssertions assertions, string because = "", params object[] becauseArgs)
        {
            return assertions.HaveStatusCode(HttpStatusCode.BadRequest, because, becauseArgs);
        }

        public static AndConstraint<HttpResponseMessage> HaveStatusCodeNotFound(this HttpResponseMessageAssertions assertions, string because = "", params object[] becauseArgs)
        {
            return assertions.HaveStatusCode(HttpStatusCode.NotFound, because, becauseArgs);
        }

        public static AndWhichConstraint<HttpResponseMessage, CommonResponse> HaveBodyAsCommonResponse(this HttpResponseMessageAssertions assertions, string because = "", params object[] becauseArgs)
        {
            return assertions.HaveBodyAsJson<CommonResponse>(because, becauseArgs);
        }

        public static void HaveBodyAsCommonResponseWithCode(this HttpResponseMessageAssertions assertions, int expected, string because = "", params object[] becauseArgs)
        {
            assertions.HaveBodyAsCommonResponse(because, becauseArgs).Which.Code.Should().Be(expected, because, becauseArgs);
        }

        public static void BePutCreated(this HttpResponseMessageAssertions assertions, string because = "", params object[] becauseArgs)
        {
            assertions.HaveStatusCodeCreated(because, becauseArgs).And.Should().HaveBodyAsCommonResponse(because, becauseArgs).Which.Should().BeEquivalentTo(CommonPutResponse.Created, because, becauseArgs);
        }

        public static void BePutModified(this HttpResponseMessageAssertions assertions, string because = "", params object[] becauseArgs)
        {
            assertions.HaveStatusCodeOk(because, becauseArgs).And.Should().HaveBodyAsCommonResponse(because, becauseArgs).Which.Should().BeEquivalentTo(CommonPutResponse.Modified, because, becauseArgs);
        }

        public static void BeDeleteDeleted(this HttpResponseMessageAssertions assertions, string because = "", params object[] becauseArgs)
        {
            assertions.HaveStatusCodeOk(because, becauseArgs).And.Should().HaveBodyAsCommonResponse(because, becauseArgs).Which.Should().BeEquivalentTo(CommonDeleteResponse.Deleted, because, becauseArgs);
        }

        public static void BeDeleteNotExist(this HttpResponseMessageAssertions assertions, string because = "", params object[] becauseArgs)
        {
            assertions.HaveStatusCodeOk(because, becauseArgs).And.Should().HaveBodyAsCommonResponse(because, becauseArgs).Which.Should().BeEquivalentTo(CommonDeleteResponse.NotExists, because, becauseArgs);
        }
    }
}
