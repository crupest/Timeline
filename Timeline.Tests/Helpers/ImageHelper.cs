using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;

namespace Timeline.Tests.Helpers
{
    public static class ImageHelper
    {
        public static byte[] CreatePngWithSize(int width, int height)
        {
            using (var image = new Image<Rgba32>(width, height))
            {
                using (var stream = new MemoryStream())
                {
                    image.SaveAsPng(stream);
                    return stream.ToArray();
                }
            }
        }
    }
}
