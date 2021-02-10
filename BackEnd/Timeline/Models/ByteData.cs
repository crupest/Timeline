using System;
using NSwag.Annotations;

namespace Timeline.Models
{
    /// <summary>
    /// Model for reading http body as bytes.
    /// </summary>
    [OpenApiFile]
    public class ByteData
    {
        /// <summary>
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="contentType">The content type.</param>
        public ByteData(byte[] data, string contentType)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));
            if (contentType is null)
                throw new ArgumentNullException(nameof(contentType));

            Data = data;
            ContentType = contentType;
        }

        /// <summary>
        /// Data.
        /// </summary>
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] Data { get; }
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        /// Content type.
        /// </summary>
        public string ContentType { get; }
    }
}
