using System.Linq;
using System.Text.RegularExpressions;

namespace Timeline.Models.Validation
{
    public class NameValidator : Validator<string>
    {
        private static Regex UniqueIdRegex { get; } = new Regex(@"^[a-zA-Z0-9]{32}$");

        public const int MaxLength = 26;

        protected override (bool, string) DoValidate(string value)
        {
            if (value.Length == 0)
            {
                return (false, Resource.NameCantBeEmpty);
            }

            if (value.Length > MaxLength)
            {
                return (false, string.Format(Resource.NameCantBeLongerThan, MaxLength));
            }

            foreach ((char c, int i) in value.Select((c, i) => (c, i)))
            {
                if (!(char.IsLetterOrDigit(c) || c == '-' || c == '_'))
                {
                    return (false, Resource.NameInvalidChar);
                }
            }

            // Currently name can't be longer than 26. So this is not needed. But reserve it for future use.
            if (UniqueIdRegex.IsMatch(value))
            {
                return (false, Resource.NameCantBeUniqueIdPattern);
            }

            return (true, GetSuccessMessage());
        }
    }
}
