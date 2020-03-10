using System;
using System.Globalization;
using static Timeline.Resources.Services.Exception;

namespace Timeline.Services
{
    [Serializable]
    public class JwtUserTokenBadFormatException : UserTokenBadFormatException
    {
        public enum ErrorKind
        {
            NoIdClaim,
            IdClaimBadFormat,
            NoVersionClaim,
            VersionClaimBadFormat,
            Other
        }

        public JwtUserTokenBadFormatException() : this("", ErrorKind.Other) { }
        public JwtUserTokenBadFormatException(string message) : base(message) { }
        public JwtUserTokenBadFormatException(string message, Exception inner) : base(message, inner) { }

        public JwtUserTokenBadFormatException(string token, ErrorKind type) : base(token, GetErrorMessage(type)) { ErrorType = type; }
        public JwtUserTokenBadFormatException(string token, ErrorKind type, Exception inner) : base(token, GetErrorMessage(type), inner) { ErrorType = type; }
        public JwtUserTokenBadFormatException(string token, ErrorKind type, string message, Exception inner) : base(token, message, inner) { ErrorType = type; }
        protected JwtUserTokenBadFormatException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public ErrorKind ErrorType { get; set; }

        private static string GetErrorMessage(ErrorKind type)
        {
            var reason = type switch
            {
                ErrorKind.NoIdClaim => JwtUserTokenBadFormatExceptionIdMissing,
                ErrorKind.IdClaimBadFormat => JwtUserTokenBadFormatExceptionIdBadFormat,
                ErrorKind.NoVersionClaim => JwtUserTokenBadFormatExceptionVersionMissing,
                ErrorKind.VersionClaimBadFormat => JwtUserTokenBadFormatExceptionVersionBadFormat,
                ErrorKind.Other => JwtUserTokenBadFormatExceptionOthers,
                _ => JwtUserTokenBadFormatExceptionUnknown
            };

            return string.Format(CultureInfo.CurrentCulture,
                Resources.Services.Exception.JwtUserTokenBadFormatException, reason);
        }
    }
}
