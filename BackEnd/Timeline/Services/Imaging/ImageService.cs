using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Timeline.Services.Imaging
{

    public class ImageService : IImageService
    {
        public async Task<IImageFormat> DetectFormatAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var format = await Task.Run(() =>
            {
                var format = Image.DetectFormat(data);
                if (format is null)
                {
                    throw new ImageException(ImageException.ErrorReason.CantDecode, data, null, null, null);
                }
                return format;
            }, cancellationToken);
            return format;
        }

        public async Task<IImageFormat> ValidateAsync(byte[] data, string? requestType = null, bool square = false, CancellationToken cancellationToken = default)
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
                        throw new ImageException(ImageException.ErrorReason.BadSize, data, requestType, format.DefaultMimeType);
                    return format;
                }
                catch (UnknownImageFormatException e)
                {
                    throw new ImageException(ImageException.ErrorReason.CantDecode, data, requestType, null, null, e);
                }
            }, cancellationToken);
            return format;
        }
    }
}
