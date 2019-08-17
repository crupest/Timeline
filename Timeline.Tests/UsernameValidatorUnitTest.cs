using FluentAssertions;
using System;
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
            var result = _validator.Validate(username, out var message);
            result.Should().BeFalse();
            return message;
        }

        private void Succeed(string username)
        {
            _validator.Validate(username, out var message).Should().BeTrue();
            message.Should().Be(ValidationConstants.SuccessMessage);
        }

        [Fact]
        public void Null()
        {
            FailAndMessage(null).Should().ContainEquivalentOf("null");
        }

        [Fact]
        public void NotString()
        {
            var result = _validator.Validate(123, out var message);
            result.Should().BeFalse();
            message.Should().ContainEquivalentOf("type");
        }

        [Fact]
        public void Empty()
        {
            FailAndMessage("").Should().ContainEquivalentOf("empty");
        }

        [Fact]
        public void WhiteSpace()
        {
            FailAndMessage(" ").Should().ContainEquivalentOf("whitespace");
            FailAndMessage("\t").Should().ContainEquivalentOf("whitespace");
            FailAndMessage("\n").Should().ContainEquivalentOf("whitespace");

            FailAndMessage("a b").Should().ContainEquivalentOf("whitespace");
            FailAndMessage("a\tb").Should().ContainEquivalentOf("whitespace");
            FailAndMessage("a\nb").Should().ContainEquivalentOf("whitespace");
        }

        [Fact]
        public void BadCharactor()
        {
            FailAndMessage("!").Should().ContainEquivalentOf("regex");
            FailAndMessage("!abc").Should().ContainEquivalentOf("regex");
            FailAndMessage("ab!c").Should().ContainEquivalentOf("regex");
        }

        [Fact]
        public void BadBegin()
        {
            FailAndMessage("-").Should().ContainEquivalentOf("regex");
            FailAndMessage("-abc").Should().ContainEquivalentOf("regex");
        }

        [Fact]
        public void TooLong()
        {
            FailAndMessage(new string('a', 40)).Should().ContainEquivalentOf("long");
        }

        [Fact]
        public void Success()
        {
            Succeed("abc");
            Succeed("_abc");
            Succeed("a-bc");
            Succeed("a-b-c");
            Succeed("a-b_c");
        }
    }
}
