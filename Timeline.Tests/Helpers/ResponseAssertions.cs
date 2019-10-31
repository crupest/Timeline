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

        public AndConstraint<HttpResponseMessageAssertions> HaveStatusCode(int expected, string because = "", params object[] becauseArgs)
        {
            return HaveStatusCode((HttpStatusCode)expected, because, becauseArgs);
        }

        public AndConstraint<HttpResponseMessageAssertions> HaveStatusCode(HttpStatusCode expected, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion.BecauseOf(because, becauseArgs)
                .ForCondition(Subject.StatusCode == expected)
                .FailWith("Expected status code of {context:HttpResponseMessage} to be {0}{reason}, but found {1}.", expected, Subject.StatusCode);
            return new AndConstraint<HttpResponseMessageAssertions>(this);
        }

        public AndWhichConstraint<HttpResponseMessageAssertions, T> HaveJsonBody<T>(string because = "", params object[] becauseArgs)
        {
            var a = Execute.Assertion.BecauseOf(because, becauseArgs);
            string body;
            try
            {
                body = Subject.Content.ReadAsStringAsync().Result;
            }
            catch (AggregateException e)
            {
                a.FailWith("Expected response body of {context:HttpResponseMessage} to be json string{reason}, but failed to read it or it was not a string. Exception is {0}.", e.InnerExceptions);
                return new AndWhichConstraint<HttpResponseMessageAssertions, T>(this, null);
            }

            var result = JsonConvert.DeserializeObject<T>(body);
            return new AndWhichConstraint<HttpResponseMessageAssertions, T>(this, result);
        }
    }

    public static class AssertionResponseExtensions
    {
        public static HttpResponseMessageAssertions Should(this HttpResponseMessage instance)
        {
            return new HttpResponseMessageAssertions(instance);
        }

        public static AndWhichConstraint<HttpResponseMessageAssertions, CommonResponse> HaveCommonBody(this HttpResponseMessageAssertions assertions, string because = "", params object[] becauseArgs)
        {
            return assertions.HaveJsonBody<CommonResponse>(because, becauseArgs);
        }

        public static AndWhichConstraint<HttpResponseMessageAssertions, CommonDataResponse<TData>> HaveCommonDataBody<TData>(this HttpResponseMessageAssertions assertions, string because = "", params object[] becauseArgs)
        {
            return assertions.HaveJsonBody<CommonDataResponse<TData>>(because, becauseArgs);
        }

        public static void BePut(this HttpResponseMessageAssertions assertions, bool create, string because = "", params object[] becauseArgs)
        {
            var body = assertions.HaveStatusCode(create ? 201 : 200, because, becauseArgs)
                .And.HaveJsonBody<CommonPutResponse>(because, becauseArgs)
                .Which;
            body.Code.Should().Be(0);
            body.Data.Create.Should().Be(create);
        }

        public static void BeDelete(this HttpResponseMessageAssertions assertions, bool delete, string because = "", params object[] becauseArgs)
        {
            var body = assertions.HaveStatusCode(200, because, becauseArgs)
                .And.HaveJsonBody<CommonDeleteResponse>(because, becauseArgs)
                .Which;
            body.Code.Should().Be(0);
            body.Data.Delete.Should().Be(delete);
        }

        public static void BeInvalidModel(this HttpResponseMessageAssertions assertions, string message = null)
        {
            message = string.IsNullOrEmpty(message) ? "" : ", " + message;
            assertions.HaveStatusCode(400, "Invalid Model Error must have 400 status code{0}", message)
                .And.HaveCommonBody("Invalid Model Error must have CommonResponse body{0}", message)
                .Which.Code.Should().Be(ErrorCodes.Http.Common.InvalidModel,
                "Invalid Model Error must have code {0} in body{1}",
                ErrorCodes.Http.Common.InvalidModel, message);
        }
    }
}
