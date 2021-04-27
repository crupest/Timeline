using System;
using System.Globalization;

namespace Timeline.Services.Imaging
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
            BadSize,
            /// <summary>
            /// Other unknown errer.
            /// </summary>
            Unknown
        }

        public ImageException() : this(null) { }
        public ImageException(string? message) : this(message, null) { }
        public ImageException(string? message, Exception? inner) : this(ErrorReason.Unknown, null, null, null, message, inner) { }

        public ImageException(ErrorReason error, byte[]? data, string? requestType, string? realType, Exception? inner) : this(error, data, requestType, realType, null, inner) { }
        public ImageException(ErrorReason error, byte[]? data, string? requestType = null, string? realType = null, string? message = null, Exception? inner = null) : base(message ?? MakeMessage(error), inner) { Error = error; ImageData = data; RequestType = requestType; RealType = realType; }

        protected ImageException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        private static string MakeMessage(ErrorReason? reason) =>
            string.Format(CultureInfo.InvariantCulture, Resource.ExceptionImage, reason switch
            {
                ErrorReason.CantDecode => Resource.ExceptionImageReasonCantDecode,
                ErrorReason.UnmatchedFormat => Resource.ExceptionImageReasonUnmatchedFormat,
                ErrorReason.BadSize => Resource.ExceptionImageReasonBadSize,
                _ => Resource.ExceptionImageReasonUnknownError
            });

        public ErrorReason Error { get; }
        public byte[]? ImageData { get; }
        public string? RequestType { get; }
        // This field will be null if decoding failed.
        public string? RealType { get; }
    }
}
