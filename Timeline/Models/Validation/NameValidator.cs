using System.Linq;
using static Timeline.Resources.Models.Validation.NameValidator;

namespace Timeline.Models.Validation
{
    public class NameValidator : Validator<string>
    {
        public const int MaxLength = 26;

        protected override (bool, string) DoValidate(string value)
        {
            if (value.Length == 0)
            {
                return (false, MessageEmptyString);
            }

            if (value.Length > MaxLength)
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
}
