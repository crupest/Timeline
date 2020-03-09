using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TimelineApp.Services
{
    public interface IImageValidator
    {
        /// <summary>
        /// Validate a image data.
        /// </summary>
        /// <param name="data">The data of the image. Can't be null.</param>
        /// <param name="requestType">If not null, the real image format will be check against the requested format and throw if not match. If null, then do not check.</param>
        /// <param name="square">If true, image must be square.</param>
        /// <returns>The format.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
        /// <exception cref="ImageException">Thrown when image data can't be decoded or real type does not match request type or image is not square when required.</exception>
        Task<IImageFormat> Validate(byte[] data, string? requestType = null, bool square = false);
    }

    public class ImageValidator : IImageValidator
    {
        public ImageValidator()
        {
        }

        public async Task<IImageFormat> Validate(byte[] data, string? requestType = null, bool square = false)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var format = await Task.Run(() =>
            {
                try
                {
                    using var image = Image.Load(data, out IImageFormat format);
                    if (requestType != null && !format.MimeTypes.Contains(requestType))
                        throw new ImageException(ImageException.ErrorReason.UnmatchedFormat, data, requestType, format.DefaultMimeType);
                    if (square && image.Width != image.Height)
                        throw new ImageException(ImageException.ErrorReason.NotSquare, data, requestType, format.DefaultMimeType);
                    return format;
                }
                catch (UnknownImageFormatException e)
                {
                    throw new ImageException(e, ImageException.ErrorReason.CantDecode, data, requestType, null);
                }
            });
            return format;
        }
    }
}
