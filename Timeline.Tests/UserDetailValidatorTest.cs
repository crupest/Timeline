using FluentAssertions;
using System.Collections.Generic;
using Timeline.Models.Validation;
using Xunit;

namespace Timeline.Tests
{
    public static class UserDetailValidatorsTest
    {
        private static void SucceedWith<TValidator>(object value) where TValidator : class, IValidator, new()
        {
            var result = new TValidator().Validate(value, out var message);
            result.Should().BeTrue();
            message.Should().Equals(ValidationConstants.SuccessMessage);
        }

        private static void FailWith<TValidator>(object value, params string[] messageContains) where TValidator : class, IValidator, new()
        {
            var result = new TValidator().Validate(value, out var message);
            result.Should().BeFalse();

            foreach (var m in messageContains)
            {
                message.Should().ContainEquivalentOf(m);
            }
        }

        public class QQ
        {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData("12345678")]
            public void Success(object qq)
            {
                SucceedWith<UserDetailValidators.QQValidator>(qq);
            }

            [Theory]
            [InlineData(123, "type")]
            [InlineData("123", "short")]
            [InlineData("111111111111111111111111111111111111", "long")]
            [InlineData("aaaaaaaa", "digit")]
            public void Fail(object qq, string messageContains)
            {
                FailWith<UserDetailValidators.QQValidator>(qq, messageContains);
            }
        }

        public class EMail
        {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData("aaa@aaa.net")]
            public void Success(object email)
            {
                SucceedWith<UserDetailValidators.EMailValidator>(email);
            }

            public static IEnumerable<object[]> FailTestData()
            {
                yield return new object[] { 123, "type" };
                yield return new object[] { new string('a', 100), "long" };
                yield return new object[] { "aaaaaaaa", "format" };
            }

            [Theory]
            [MemberData(nameof(FailTestData))]
            public void Fail(object email, string messageContains)
            {
                FailWith<UserDetailValidators.EMailValidator>(email, messageContains);
            }
        }

        public class PhoneNumber
        {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData("12345678910")]
            public void Success(object phoneNumber)
            {
                SucceedWith<UserDetailValidators.PhoneNumberValidator>(phoneNumber);
            }

            [Theory]
            [InlineData(123, "type")]
            [InlineData("111111111111111111111111111111111111", "long")]
            [InlineData("aaaaaaaa", "digit")]
            public void Fail(object phoneNumber, string messageContains)
            {
                FailWith<UserDetailValidators.PhoneNumberValidator>(phoneNumber, messageContains);
            }
        }
    }
}
