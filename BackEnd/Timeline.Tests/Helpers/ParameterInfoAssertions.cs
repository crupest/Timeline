using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Formatting;
using FluentAssertions.Primitives;
using System;
using System.Reflection;

namespace Timeline.Tests.Helpers
{
    public class ParameterInfoValueFormatter : IValueFormatter
    {
        public bool CanHandle(object value)
        {
            return value is ParameterInfo;
        }

        public string Format(object value, FormattingContext context, FormatChild formatChild)
        {
            var param = (ParameterInfo)value;
            return $"{param.Member.DeclaringType.FullName}.{param.Member.Name}#{param.Name}";
        }
    }

    public class ParameterInfoAssertions : ReferenceTypeAssertions<ParameterInfo, ParameterInfoAssertions>
    {
        static ParameterInfoAssertions()
        {
            Formatter.AddFormatter(new ParameterInfoValueFormatter());
        }

        public ParameterInfoAssertions(ParameterInfo parameterInfo)
        {
            Subject = parameterInfo;
        }

        protected override string Identifier => "parameter";

        public AndWhichConstraint<ParameterInfoAssertions, TAttribute> BeDecoratedWith<TAttribute>(string because = "", params object[] becauseArgs)
            where TAttribute : Attribute
        {
            var attribute = Subject.GetCustomAttribute<TAttribute>(false);

            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(attribute != null)
                .FailWith("Expected {0} {1} to be decorated with {2}{reason}, but that attribute was not found.",
                Identifier, Subject, typeof(TAttribute).FullName);

            return new AndWhichConstraint<ParameterInfoAssertions, TAttribute>(this, attribute);
        }
    }

    public static class ParameterInfoAssertionExtensions
    {
        public static ParameterInfoAssertions Should(this ParameterInfo parameterInfo)
        {
            return new ParameterInfoAssertions(parameterInfo);
        }
    }
}
