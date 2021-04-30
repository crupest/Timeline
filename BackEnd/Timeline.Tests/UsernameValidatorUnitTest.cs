using FluentAssertions;
using Timeline.Models.Validation;
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
        [InlineData("ab c！")] // This is a chinese ! .
        public void BadCharactor(string value)
        {
            FailAndMessage(value).Should().ContainEquivalentOf("only consists of");
        }

        [Fact]
        public void TooLong()
        {
            FailAndMessage(new string('a', 40)).Should().ContainEquivalentOf("long");
        }

        [Fact(Skip = "Currently name can't be longer than 26. So this will print message of too long.")]
        public void UniqueId()
        {
            FailAndMessage("e4c80127d092d9b2fc19c5e04612d4c0").Should().ContainEquivalentOf("unique id");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("abc")]
        [InlineData("-abc")]
        [InlineData("_abc")]
        [InlineData("abc-")]
        [InlineData("abc_")]
        [InlineData("a-bc")]
        [InlineData("a-b-c")]
        [InlineData("a-b_c")]
        [InlineData("a-你好_c")]
        public void Success(string value)
        {
            var (result, _) = _validator.Validate(value);
            result.Should().BeTrue();
        }
    }
}
