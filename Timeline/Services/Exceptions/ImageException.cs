using System;
using System.Globalization;

namespace Timeline.Services.Exceptions
{
    [Serializable]
    public class ImageException : Exception
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
            /// Image is not of required size.
            /// </summary>
            NotSquare,
            /// <summary>
            /// Other unknown errer.
            /// </summary>
            Unknown
        }

        public ImageException() : this(null) { }
        public ImageException(string? message) : this(message, null) { }
        public ImageException(string? message, Exception? inner) : this(ErrorReason.Unknown, null, null, null, message, inner) { }

        public ImageException(ErrorReason error, byte[]? data, string? requestType = null, string? realType = null, string? message = null, Exception? inner = null) : base(MakeMessage(error).AppendAdditionalMessage(message), inner) { Error = error; ImageData = data; RequestType = requestType; RealType = realType; }

        protected ImageException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        private static string MakeMessage(ErrorReason? reason) =>
            string.Format(CultureInfo.InvariantCulture, Resources.Services.Exceptions.ImageException, reason switch
            {
                ErrorReason.CantDecode => Resources.Services.Exceptions.ImageExceptionCantDecode,
                ErrorReason.UnmatchedFormat => Resources.Services.Exceptions.ImageExceptionUnmatchedFormat,
                ErrorReason.NotSquare => Resources.Services.Exceptions.ImageExceptionBadSize,
                _ => Resources.Services.Exceptions.ImageExceptionUnknownError
            });

        public ErrorReason Error { get; }
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[]? ImageData { get; }
#pragma warning restore CA1819 // Properties should not return arrays
        public string? RequestType { get; }

        // This field will be null if decoding failed.
        public string? RealType { get; }
    }
}
