using Microsoft.IdentityModel.Tokens;
using System;
using System.Globalization;
using static Timeline.Resources.Services.Exception;

namespace Timeline.Services
{
    [Serializable]
    public class JwtVerifyException : Exception
    {
        public static class ErrorCodes
        {
            // Codes in -1000 ~ -1999 usually means the user provides a token that is not created by this server.

            public const int Others = -1001;
            public const int NoIdClaim = -1002;
            public const int IdClaimBadFormat = -1003;
            public const int NoVersionClaim = -1004;
            public const int VersionClaimBadFormat = -1005;

            /// <summary>
            /// Corresponds to <see cref="SecurityTokenExpiredException"/>.
            /// </summary>
            public const int Expired = -2001;
            public const int OldVersion = -2002;
        }

        public JwtVerifyException() : base(GetErrorMessage(0)) { }
        public JwtVerifyException(string message) : base(message) { }
        public JwtVerifyException(string message, Exception inner) : base(message, inner) { }

        public JwtVerifyException(int code) : base(GetErrorMessage(code)) { ErrorCode = code; }
        public JwtVerifyException(string message, int code) : base(message) { ErrorCode = code; }
        public JwtVerifyException(Exception inner, int code) : base(GetErrorMessage(code), inner) { ErrorCode = code; }
        public JwtVerifyException(string message, Exception inner, int code) : base(message, inner) { ErrorCode = code; }
        protected JwtVerifyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public int ErrorCode { get; set; }

        private static string GetErrorMessage(int errorCode)
        {
            var reason = errorCode switch
            {
                ErrorCodes.Others => JwtVerifyExceptionOthers,
                ErrorCodes.NoIdClaim => JwtVerifyExceptionNoIdClaim,
                ErrorCodes.IdClaimBadFormat => JwtVerifyExceptionIdClaimBadFormat,
                ErrorCodes.NoVersionClaim => JwtVerifyExceptionNoVersionClaim,
                ErrorCodes.VersionClaimBadFormat => JwtVerifyExceptionVersionClaimBadFormat,
                ErrorCodes.Expired => JwtVerifyExceptionExpired,
                ErrorCodes.OldVersion => JwtVerifyExceptionOldVersion,
                _ => JwtVerifyExceptionUnknown
            };

            return string.Format(CultureInfo.InvariantCulture, Resources.Services.Exception.JwtVerifyException, reason);
        }
    }
}
