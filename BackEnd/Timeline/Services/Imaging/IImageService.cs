using SixLabors.ImageSharp.Formats;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Timeline.Services.Imaging
{
    public interface IImageService
    {
        /// <summary>
        /// Detect the format of a image.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The image format.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
        /// <exception cref="ImageException">Thrown when image data can't be detected.</exception>
        Task<IImageFormat> DetectFormatAsync(byte[] data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validate a image data.
        /// </summary>
        /// <param name="data">The data of the image. Can't be null.</param>
        /// <param name="requestType">If not null, the real image format will be check against the requested format and throw if not match. If null, then do not check.</param>
        /// <param name="square">If true, image must be square.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The format.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
        /// <exception cref="ImageException">Thrown when image data can't be decoded or real type does not match request type or image is not square when required.</exception>
        Task<IImageFormat> ValidateAsync(byte[] data, string? requestType = null, bool square = false, CancellationToken cancellationToken = default);
    }
}
