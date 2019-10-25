using System;
using System.Linq;
using static Timeline.Resources.Models.Validation.UsernameValidator;

namespace Timeline.Models.Validation
{
    public class UsernameValidator : Validator<string>
    {
        public const int MaxLength = 26;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Already checked in base class.")]
        protected override (bool, string) DoValidate(string value)
        {
            if (value.Length == 0)
            {
                return (false, MessageEmptyString);
            }

            if (value.Length > 26)
            {
                return (false, MessageTooLong);
            }

            foreach ((char c, int i) in value.Select((c, i) => (c, i)))
            {
                if (!(char.IsLetterOrDigit(c) || c == '-' || c == '_'))
                {
                    return (false, MessageInvalidChar);
                }
            }

            return (true, GetSuccessMessage());
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
