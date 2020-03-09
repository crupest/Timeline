using System;
using System.Globalization;

namespace TimelineApp.Services
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
            /// Image is not a square.
            /// </summary>
            NotSquare
        }

        public ImageException() : base(MakeMessage(null)) { }
        public ImageException(string message) : base(message) { }
        public ImageException(string message, Exception inner) : base(message, inner) { }

        public ImageException(ErrorReason error, byte[]? data = null, string? requestType = null, string? realType = null) : base(MakeMessage(error)) { Error = error; ImageData = data; RequestType = requestType; RealType = realType; }
        public ImageException(Exception inner, ErrorReason error, byte[]? data = null, string? requestType = null, string? realType = null) : base(MakeMessage(error), inner) { Error = error; ImageData = data; RequestType = requestType; RealType = realType; }

        protected ImageException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        private static string MakeMessage(ErrorReason? reason) =>
            string.Format(CultureInfo.InvariantCulture, Resources.Services.Exception.ImageException, reason switch
            {
                ErrorReason.CantDecode => Resources.Services.Exception.ImageExceptionCantDecode,
                ErrorReason.UnmatchedFormat => Resources.Services.Exception.ImageExceptionUnmatchedFormat,
                ErrorReason.NotSquare => Resources.Services.Exception.ImageExceptionBadSize,
                _ => Resources.Services.Exception.ImageExceptionUnknownError
            });

        public ErrorReason? Error { get; }
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[]? ImageData { get; }
#pragma warning restore CA1819 // Properties should not return arrays
        public string? RequestType { get; }

        // This field will be null if decoding failed.
        public string? RealType { get; }
    }
}
