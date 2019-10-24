using FluentAssertions;
using FluentAssertions.Primitives;
using FluentAssertions.Specialized;
using System;
using System.Threading.Tasks;

namespace Timeline.Tests.Helpers
{
    public static class AsyncFunctionAssertionsExtensions
    {
        public static async Task<AndConstraint<ObjectAssertions>> ThrowAsync(this AsyncFunctionAssertions assertions, Type exceptionType, string because = "", params object[] becauseArgs)
        {
            return (await assertions.ThrowAsync<Exception>(because, becauseArgs)).Which.Should().BeAssignableTo(exceptionType);
        }
    }
}
