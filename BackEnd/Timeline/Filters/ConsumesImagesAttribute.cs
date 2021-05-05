using Microsoft.AspNetCore.Mvc;
using Timeline.Models;

namespace Timeline.Filters
{
    public class ConsumesImagesAttribute : ConsumesAttribute
    {
        public ConsumesImagesAttribute()
            : base(MimeTypes.ImagePng,
                  MimeTypes.ImageJpeg,
                  MimeTypes.ImageGif,
                  MimeTypes.ImageWebp)
        {

        }
    }
}
