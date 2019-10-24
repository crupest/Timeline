using System;
using System.Globalization;

namespace Timeline.Services
{
    /// <summary>
    /// Thrown when avatar is of bad format.
    /// </summary>
    [Serializable]
    public class AvatarFormatException : Exception
    {
        public enum ErrorReason
        {
            /// <summary>
            /// Decoding image failed.
            /// </summary>
            CantDecode,
            /// <summary>
            /// Decoding succeeded but the real type is not the specified type.
            /// </summary>
            UnmatchedFormat,
            /// <summary>
            /// Image is not a square.
            /// </summary>
            BadSize
        }

        public AvatarFormatException() : base(MakeMessage(null)) { }
        public AvatarFormatException(string message) : base(message) { }
        public AvatarFormatException(string message, Exception inner) : base(message, inner) { }

        public AvatarFormatException(Avatar avatar, ErrorReason error) : base(MakeMessage(error)) { Avatar = avatar; Error = error; }
        public AvatarFormatException(Avatar avatar, ErrorReason error, Exception inner) : base(MakeMessage(error), inner) { Avatar = avatar; Error = error; }

        protected AvatarFormatException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        private static string MakeMessage(ErrorReason? reason) =>
            string.Format(CultureInfo.InvariantCulture, Resources.Services.Exception.AvatarFormatException, reason switch
            {
                ErrorReason.CantDecode => Resources.Services.Exception.AvatarFormatExceptionCantDecode,
                ErrorReason.UnmatchedFormat => Resources.Services.Exception.AvatarFormatExceptionUnmatchedFormat,
                ErrorReason.BadSize => Resources.Services.Exception.AvatarFormatExceptionBadSize,
                _ => Resources.Services.Exception.AvatarFormatExceptionUnknownError
            });

        public ErrorReason? Error { get; set; }
        public Avatar? Avatar { get; set; }
    }
}
