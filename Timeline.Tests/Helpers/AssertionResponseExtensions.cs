using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Formatting;
using FluentAssertions.Primitives;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using Timeline.Models.Http;

namespace Timeline.Tests.Helpers
{
    public class HttpResponseMessageValueFormatter : IValueFormatter
    {
        public bool CanHandle(object value)
        {
            return value is HttpResponseMessage;
        }

        public string Format(object value, FormattingContext context, FormatChild formatChild)
        {
            string newline = context.UseLineBreaks ? Environment.NewLine : "";
            string padding = new string('\t', context.Depth);

            var res = (HttpResponseMessage)value;
            return $"{newline}{padding} Status Code: {res.StatusCode} ; Body: {res.Content.ReadAsStringAsync().Result} ;";
        }
    }

    public class HttpResponseMessageAssertions
            : ReferenceTypeAssertions<HttpResponseMessage, HttpResponseMessageAssertions>
    {
        static HttpResponseMessageAssertions()
        {
            Formatter.AddFormatter(new HttpResponseMessageValueFormatter());
        }

        public HttpResponseMessageAssertions(HttpResponseMessage instance)
        {
            Subject = instance;
        }

        protected override string Identifier => "HttpResponseMessage";

        public AndConstraint<HttpResponseMessage> HaveStatusCode(HttpStatusCode expected, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion.BecauseOf(because, becauseArgs)
                .ForCondition(Subject.StatusCode == expected)
                .FailWith("Expected status code of {context:HttpResponseMessage} to be {0}{reason}, but found {1}.\nResponse is {2}.", expected, Subject.StatusCode, Subject);
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
                a.FailWith("Failed to read response body of {context:HttpResponseMessage}{reason}.\nException is {0}.", e);
                return new AndWhichConstraint<HttpResponseMessage, T>(Subject, null);
            }

            try
            {
                var result = JsonConvert.DeserializeObject<T>(body);
                return new AndWhichConstraint<HttpResponseMessage, T>(Subject, result);
            }
            catch (Exception e)
            {
                a.FailWith("Failed to convert response body of {context:HttpResponseMessage} to {0}{reason}.\nResponse is {1}.\nException is {2}.", typeof(T).FullName, Subject, e);
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
