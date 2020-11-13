using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Formatting;
using FluentAssertions.Primitives;
using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Timeline.Models.Converters;
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
            return $"{newline}{padding} Status Code: {res.StatusCode}";
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

        public async Task<T> HaveAndGetJsonBodyAsync<T>(string because = "", params object[] becauseArgs)
        {
            var a = Execute.Assertion.BecauseOf(because, becauseArgs);

            var body = await Subject.ReadBodyAsJsonAsync<T>();
            if (body == null)
            {
                a.FailWith("Expected response body of {context:HttpResponseMessage} to be json string of type {0}{reason}, but failed to read it or it was not a valid json string.", typeof(T).FullName);
                return default!;
            }
            return body;
        }
    }

    public static class AssertionResponseExtensions
    {
        public static HttpResponseMessageAssertions Should(this HttpResponseMessage instance)
        {
            return new HttpResponseMessageAssertions(instance);
        }

        public static Task<CommonResponse> HaveAndGetCommonBodyAsync(this HttpResponseMessageAssertions assertions, string because = "", params object[] becauseArgs)
        {
            return assertions.HaveAndGetJsonBodyAsync<CommonResponse>(because, becauseArgs);
        }

        public static async Task HaveCommonBodyWithCodeAsync(this HttpResponseMessageAssertions assertions, int code, string? message = null, params object[] messageArgs)
        {
            message = string.IsNullOrEmpty(message) ? "" : ", " + string.Format(CultureInfo.CurrentCulture, message, messageArgs);
            var body = await assertions.HaveAndGetCommonBodyAsync("Response body should be CommonResponse{0}", message);
            body.Code.Should().Be(code, "Response body code is not the specified one{0}", message);
        }

        public static Task<CommonDataResponse<TData>> HaveAndGetCommonDataBodyAsync<TData>(this HttpResponseMessageAssertions assertions, string because = "", params object[] becauseArgs)
        {
            return assertions.HaveAndGetJsonBodyAsync<CommonDataResponse<TData>>(because, becauseArgs);
        }

        public static async Task BePutAsync(this HttpResponseMessageAssertions assertions, bool create, string because = "", params object[] becauseArgs)
        {
            var body = await assertions.HaveStatusCode(create ? 201 : 200, because, becauseArgs)
                .And.HaveAndGetJsonBodyAsync<CommonPutResponse>(because, becauseArgs);
            body.Code.Should().Be(0);
            body.Data.Create.Should().Be(create);
        }

        public static async Task BeDeleteAsync(this HttpResponseMessageAssertions assertions, bool delete, string because = "", params object[] becauseArgs)
        {
            var body = await assertions.HaveStatusCode(200, because, becauseArgs)
                .And.HaveAndGetJsonBodyAsync<CommonDeleteResponse>(because, becauseArgs);
            body.Code.Should().Be(0);
            body.Data.Delete.Should().Be(delete);
        }

        public static async Task BeInvalidModelAsync(this HttpResponseMessageAssertions assertions, string message = null)
        {
            message = string.IsNullOrEmpty(message) ? "" : ", " + message;
            var body = await assertions.HaveStatusCode(400, "Invalid Model Error must have 400 status code{0}", message)
                .And.HaveAndGetCommonBodyAsync("Invalid Model Error must have CommonResponse body{0}", message);
            body.Code.Should().Be(ErrorCodes.Common.InvalidModel,
                "Invalid Model Error must have code {0} in body{1}",
                ErrorCodes.Common.InvalidModel, message);
        }
    }
}
