using FluentAssertions;
using Timeline.Models.Validation;
using Timeline.Tests.Helpers;
using Xunit;

namespace Timeline.Tests
{
    public class UsernameValidatorUnitTest : IClassFixture<UsernameValidator>
    {
        private readonly UsernameValidator _validator;

        public UsernameValidatorUnitTest(UsernameValidator validator)
        {
            _validator = validator;
        }

        private string FailAndMessage(string username)
        {
            var (result, message) = _validator.Validate(username);
            result.Should().BeFalse();
            return message;
        }

        [Fact]
        public void Null()
        {
            FailAndMessage(null).Should().ContainEquivalentOf("null");
        }

        [Fact]
        public void NotString()
        {
            var (result, message) = _validator.Validate(123);
            result.Should().BeFalse();
            message.Should().ContainEquivalentOf("type");
        }

        [Fact]
        public void Empty()
        {
            FailAndMessage("").Should().ContainEquivalentOf("empty");
        }

        [Theory]
        [InlineData("!")]
        [InlineData("!abc")]
        [InlineData("ab c")]
        public void BadCharactor(string value)
        {
            FailAndMessage(value).Should().ContainEquivalentOf("invalid")
                .And.ContainEquivalentOf("character");
        }

        [Fact]
        public void TooLong()
        {
            FailAndMessage(new string('a', 40)).Should().ContainEquivalentOf("long");
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("-abc")]
        [InlineData("_abc")]
        [InlineData("abc-")]
        [InlineData("abc_")]
        [InlineData("a-bc")]
        [InlineData("a-b-c")]
        [InlineData("a-b_c")]
        public void Success(string value)
        {

            var (result, _) = _validator.Validate(value);
            result.Should().BeTrue();
        }
    }
}
