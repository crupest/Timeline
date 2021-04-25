using System;

namespace Timeline.Services.User
{
    [Serializable]
    public class PasswordBadFormatException : Exception
    {
        public PasswordBadFormatException() : base(Resources.Services.Exception.PasswordBadFormatException) { }
        public PasswordBadFormatException(string message) : base(message) { }
        public PasswordBadFormatException(string message, Exception inner) : base(message, inner) { }

        public PasswordBadFormatException(string password, string validationMessage) : this()
        {
            Password = password;
            ValidationMessage = validationMessage;
        }

        protected PasswordBadFormatException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public string Password { get; set; } = "";

        public string ValidationMessage { get; set; } = "";
    }
}
