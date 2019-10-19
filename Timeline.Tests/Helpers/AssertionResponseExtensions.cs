using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Formatting;
using FluentAssertions.Primitives;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
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

            var builder = new StringBuilder();
            builder.Append($"{newline}{padding} Status Code: {res.StatusCode} ; Body: ");

            try
            {
                var body = res.Content.ReadAsStringAsync().Result;
                if (body.Length > 40)
                {
                    body = body[0..40] + " ...";
                }
                builder.Append(body);
            }
            catch (AggregateException)
            {
                builder.Append("NOT A STRING.");
            }

            return builder.ToString();
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

        public AndConstraint<HttpResponseMessage> HaveStatusCode(int expected, string because = "", params object[] becauseArgs)
        {
            return HaveStatusCode((HttpStatusCode)expected, because, becauseArgs);
        }

        public AndConstraint<HttpResponseMessage> HaveStatusCode(HttpStatusCode expected, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion.BecauseOf(because, becauseArgs)
                .ForCondition(Subject.StatusCode == expected)
                .FailWith("Expected status code of {context:HttpResponseMessage} to be {0}{reason}, but found {1}.", expected, Subject.StatusCode);
            return new AndConstraint<HttpResponseMessage>(Subject);
        }

        public AndWhichConstraint<HttpResponseMessage, T> HaveJsonBody<T>(string because = "", params object[] becauseArgs)
        {
            var a = Execute.Assertion.BecauseOf(because, becauseArgs);
            string body;
            try
            {
                body = Subject.Content.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                a.FailWith("Expected response body of {context:HttpResponseMessage} to be json string{reason}, but failed to read it or it was not a string. Exception is {0}.", e);
                return new AndWhichConstraint<HttpResponseMessage, T>(Subject, null);
            }

            try
            {
                var result = JsonConvert.DeserializeObject<T>(body);
                return new AndWhichConstraint<HttpResponseMessage, T>(Subject, result);
            }
            catch (Exception e)
            {
                a.FailWith("Expected response body of {context:HttpResponseMessage} to be able to convert to {0} instance{reason}, but failed. Exception is {1}.", typeof(T).FullName, e);
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

        public static AndWhichConstraint<HttpResponseMessage, CommonResponse> HaveCommonBody(this HttpResponseMessageAssertions assertions, string because = "", params object[] becauseArgs)
        {
            return assertions.HaveJsonBody<CommonResponse>(because, becauseArgs);
        }

        public static AndWhichConstraint<HttpResponseMessage, CommonDataResponse<TData>> HaveCommonDataBody<TData>(this HttpResponseMessageAssertions assertions, string because = "", params object[] becauseArgs)
        {
            return assertions.HaveJsonBody<CommonDataResponse<TData>>(because, becauseArgs);
        }

        public static void BePutCreate(this HttpResponseMessageAssertions assertions, string because = "", params object[] becauseArgs)
        {
            assertions.HaveStatusCode(201, because, becauseArgs)
                .And.Should().HaveCommonDataBody<CommonPutResponse.ResponseData>(because, becauseArgs).Which.Should().BeEquivalentTo(CommonPutResponse.Create(), because, becauseArgs);
        }

        public static void BePutModify(this HttpResponseMessageAssertions assertions, string because = "", params object[] becauseArgs)
        {
            assertions.HaveStatusCode(200, because, becauseArgs)
                .And.Should().HaveCommonDataBody<CommonPutResponse.ResponseData>(because, becauseArgs).Which.Should().BeEquivalentTo(CommonPutResponse.Modify(), because, becauseArgs);
        }

        public static void BeDeleteDelete(this HttpResponseMessageAssertions assertions, string because = "", params object[] becauseArgs)
        {
            assertions.HaveStatusCode(200, because, becauseArgs)
                .And.Should().HaveCommonDataBody<CommonDeleteResponse.ResponseData>(because, becauseArgs).Which.Should().BeEquivalentTo(CommonDeleteResponse.Delete(), because, becauseArgs);
        }

        public static void BeDeleteNotExist(this HttpResponseMessageAssertions assertions, string because = "", params object[] becauseArgs)
        {
            assertions.HaveStatusCode(200, because, becauseArgs)
                .And.Should().HaveCommonDataBody<CommonDeleteResponse.ResponseData>(because, becauseArgs).Which.Should().BeEquivalentTo(CommonDeleteResponse.NotExist(), because, becauseArgs);
        }

        public static void BeInvalidModel(this HttpResponseMessageAssertions assertions, string message = null)
        {
            message = string.IsNullOrEmpty(message) ? "" : ", " + message;
            assertions.HaveStatusCode(400, "Invalid Model Error must have 400 status code{0}", message)
                .And.Should().HaveCommonBody("Invalid Model Error must have CommonResponse body{0}", message)
                .Which.Code.Should().Be(ErrorCodes.Http.Common.InvalidModel,
                "Invalid Model Error must have code {0} in body{1}",
                ErrorCodes.Http.Common.InvalidModel, message);
        }
    }
}
