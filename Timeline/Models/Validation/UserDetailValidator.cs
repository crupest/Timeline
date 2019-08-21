using System;
using System.Net.Mail;

namespace Timeline.Models.Validation
{
    public abstract class OptionalStringValidator : IValidator
    {
        public bool Validate(object value, out string message)
        {
            if (value == null)
            {
                message = ValidationConstants.SuccessMessage;
                return true;
            }

            if (value is string s)
            {
                if (s.Length == 0)
                {
                    message = ValidationConstants.SuccessMessage;
                    return true;
                }
                return DoValidate(s, out message);
            }
            else
            {
                message = "Value is not of type string.";
                return false;
            }
        }

        protected abstract bool DoValidate(string value, out string message);
    }

    public static class UserDetailValidators
    {

        public class QQValidator : OptionalStringValidator
        {
            protected override bool DoValidate(string value, out string message)
            {
                if (value.Length < 5)
                {
                    message = "QQ is too short.";
                    return false;
                }

                if (value.Length > 11)
                {
                    message = "QQ is too long.";
                    return false;
                }

                foreach (var c in value)
                {
                    if (!char.IsDigit(c))
                    {
                        message = "QQ must only contain digit.";
                        return false;
                    }
                }

                message = ValidationConstants.SuccessMessage;
                return true;
            }
        }

        public class EMailValidator : OptionalStringValidator
        {
            protected override bool DoValidate(string value, out string message)
            {
                if (value.Length > 50)
                {
                    message = "E-Mail is too long.";
                    return false;
                }

                try
                {
                    var _ = new MailAddress(value);
                }
                catch (FormatException)
                {
                    message = "The format of E-Mail is bad.";
                    return false;
                }
                message = ValidationConstants.SuccessMessage;
                return true;
            }
        }

        public class PhoneNumberValidator : OptionalStringValidator
        {
            protected override bool DoValidate(string value, out string message)
            {
                if (value.Length > 14)
                {
                    message = "Phone number is too long.";
                    return false;
                }

                foreach (var c in value)
                {
                    if (!char.IsDigit(c))
                    {
                        message = "Phone number can only contain digit.";
                        return false;
                    }
                }

                message = ValidationConstants.SuccessMessage;
                return true;
            }
        }
    }
}
