using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;

namespace TimelineApp.Tests.Helpers
{
    public static class ImageHelper
    {
        public static byte[] CreatePngWithSize(int width, int height)
        {
            using var image = new Image<Rgba32>(width, height);
            using var stream = new MemoryStream();
            image.SaveAsPng(stream);
            return stream.ToArray();
        }

        public static byte[] CreateImageWithSize(int width, int height, IImageFormat format)
        {
            using var image = new Image<Rgba32>(width, height);
            using var stream = new MemoryStream();
            image.Save(stream, format);
            return stream.ToArray();
        }
    }
}
