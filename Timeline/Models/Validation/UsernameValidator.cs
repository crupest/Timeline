using System.Linq;
using System.Text.RegularExpressions;

namespace Timeline.Models.Validation
{
    public class UsernameValidator : Validator<string>
    {
        public const int MaxLength = 26;
        public const string RegexPattern = @"^[a-zA-Z0-9_][a-zA-Z0-9-_]*$";

        private readonly Regex _regex = new Regex(RegexPattern);

        protected override bool DoValidate(string value, out string message)
        {
            if (value.Length == 0)
            {
                message = "An empty string is not permitted.";
                return false;
            }

            if (value.Length > 26)
            {
                message = $"Too long, more than 26 characters is not premitted, found {value.Length}.";
                return false;
            }

            foreach ((char c, int i) in value.Select((c, i) => (c, i)))
                if (char.IsWhiteSpace(c))
                {
                    message = $"A whitespace is found at {i} . Whitespace is not permited.";
                    return false;
                }

            var match = _regex.Match(value);
            if (!match.Success)
            {
                message = "Regex match failed.";
                return false;
            }

            message = ValidationConstants.SuccessMessage;
            return true;
        }
    }
}
