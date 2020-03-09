using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TimelineApp.Models.Http;
using Xunit;
using Xunit.Abstractions;

namespace TimelineApp.Tests
{
    public class ErrorCodeTest
    {
        private readonly ITestOutputHelper _output;

        public ErrorCodeTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ShouldWork()
        {
            var errorCodes = new Dictionary<int, string>();

            void RecursiveCheckErrorCode(Type type)
            {
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(int)))
                {
                    var name = type.FullName + "." + field.Name;
                    var value = (int)field.GetRawConstantValue();
                    _output.WriteLine($"Find error code {name} , value is {value}.");

                    value.Should().BeInRange(1000_0000, 9999_9999, "Error code should have exactly 8 digits.");

                    errorCodes.Should().NotContainKey(value,
                        "identical error codes are found and conflict paths are {0} and {1}",
                        name, errorCodes.GetValueOrDefault(value));

                    errorCodes.Add(value, name);
                }

                foreach (var nestedType in type.GetNestedTypes())
                {
                    RecursiveCheckErrorCode(nestedType);
                }
            }

            RecursiveCheckErrorCode(typeof(ErrorCodes));
        }
    }
}
