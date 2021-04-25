using System;
using System.Globalization;

namespace Timeline.Services.Token
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
            NoExp,
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
                ErrorKind.NoIdClaim => Resource.ExceptionJwtUserTokenBadFormatReasonIdMissing,
                ErrorKind.IdClaimBadFormat => Resource.ExceptionJwtUserTokenBadFormatReasonIdBadFormat,
                ErrorKind.NoVersionClaim => Resource.ExceptionJwtUserTokenBadFormatReasonVersionMissing,
                ErrorKind.VersionClaimBadFormat => Resource.ExceptionJwtUserTokenBadFormatReasonVersionBadFormat,
                ErrorKind.Other => Resource.ExceptionJwtUserTokenBadFormatReasonOthers,
                _ => Resource.ExceptionJwtUserTokenBadFormatReasonUnknown
            };

            return string.Format(CultureInfo.CurrentCulture, Resource.ExceptionJwtUserTokenBadFormat, reason);
        }
    }
}
