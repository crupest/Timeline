using Microsoft.AspNetCore.Mvc;
using Timeline.Models;

namespace Timeline.Filters
{
    public class ProducesImagesAttribute : ProducesAttribute
    {
        public ProducesImagesAttribute()
            : base(MimeTypes.ImagePng,
                  MimeTypes.ImageJpeg,
                  MimeTypes.ImageGif,
                  MimeTypes.ImageWebp,
                  MimeTypes.TextJson,
                  MimeTypes.ApplicationJson)
        {

        }
    }
}
