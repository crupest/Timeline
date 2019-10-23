using System;
using System.Linq;

namespace Timeline.Models.Validation
{
    public class UsernameValidator : Validator<string>
    {
        public const int MaxLength = 26;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Already checked in base class.")]
        protected override (bool, ValidationMessageGenerator) DoValidate(string value)
        {
            if (value.Length == 0)
            {
                return (false, factory =>
                    factory?.Create(typeof(UsernameValidator))?["ValidationMessageEmptyString"]
                    ?? Resources.Models.Validation.UsernameValidator.InvariantValidationMessageEmptyString);
            }

            if (value.Length > 26)
            {
                return (false, factory =>
                    factory?.Create(typeof(UsernameValidator))?["ValidationMessageTooLong"]
                    ?? Resources.Models.Validation.UsernameValidator.InvariantValidationMessageTooLong);
            }

            foreach ((char c, int i) in value.Select((c, i) => (c, i)))
            {
                if (!(char.IsLetterOrDigit(c) || c == '-' || c == '_'))
                {
                    return (false, factory =>
                        factory?.Create(typeof(UsernameValidator))?["ValidationMessageInvalidChar"]
                        ?? Resources.Models.Validation.UsernameValidator.InvariantValidationMessageInvalidChar);
                }
            }

            return (true, SuccessMessageGenerator);
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
        AllowMultiple = false)]
    public class UsernameAttribute : ValidateWithAttribute
    {
        public UsernameAttribute()
            : base(typeof(UsernameValidator))
        {

        }
    }
}
