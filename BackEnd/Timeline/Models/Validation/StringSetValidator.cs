using System;
using System.Linq;

namespace Timeline.Models.Validation
{
    public class StringSetValidator : Validator<string>
    {
        public StringSetValidator(params string[] set)
        {
            Set = set;
        }

#pragma warning disable CA1819 // Properties should not return arrays
        public string[] Set { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

        protected override (bool, string) DoValidate(string value)
        {
            var contains = Set.Contains(value, StringComparer.OrdinalIgnoreCase);
            if (!contains)
            {
                return (false, "Not a valid value.");
            }
            else
            {
                return (true, GetSuccessMessage());
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
    AllowMultiple = false)]
    public class ValidationSetAttribute : ValidateWithAttribute
    {
        public ValidationSetAttribute(params string[] set) : base(new StringSetValidator(set))
        {

        }
    }
}
