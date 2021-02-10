using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Timeline.Models;

namespace Timeline.Formatters
{
    /// <summary>
    /// Formatter that reads body as byte data.
    /// </summary>
    public class ByteDataInputFormatter : InputFormatter
    {
        /// <summary>
        /// 
        /// </summary>
        public ByteDataInputFormatter()
        {
            SupportedMediaTypes.Add(MimeTypes.ImagePng);
            SupportedMediaTypes.Add(MimeTypes.ImageJpeg);
            SupportedMediaTypes.Add(MimeTypes.ImageGif);
            SupportedMediaTypes.Add(MimeTypes.ImageWebp);
            SupportedMediaTypes.Add(MimeTypes.TextPlain);
            SupportedMediaTypes.Add(MimeTypes.TextMarkdown);
        }

        /// <inheritdoc/>
        public override bool CanRead(InputFormatterContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (context.ModelType == typeof(ByteData))
                return true;

            return false;
        }

        /// <inheritdoc/>
        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            var contentLength = request.ContentLength;

            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<ByteDataInputFormatter>>();

            if (contentLength == null)
            {
                logger.LogInformation("Failed to read body as bytes. Content-Length is not set.");
                return await InputFormatterResult.FailureAsync();
            }

            if (contentLength == 0)
            {
                logger.LogInformation("Failed to read body as bytes. Content-Length is 0.");
                return await InputFormatterResult.FailureAsync();
            }

            var bodyStream = request.Body;

            var data = new byte[contentLength.Value];
            var bytesRead = await bodyStream.ReadAsync(data);

            if (bytesRead != contentLength)
            {
                logger.LogInformation("Failed to read body as bytes. Actual length of body is smaller than Content-Length.");
                return await InputFormatterResult.FailureAsync();
            }

            var extraByte = new byte[1];
            if (await bodyStream.ReadAsync(extraByte) != 0)
            {
                logger.LogInformation("Failed to read body as bytes. Actual length of body is greater than Content-Length.");
                return await InputFormatterResult.FailureAsync();
            }

            return await InputFormatterResult.SuccessAsync(new ByteData(data, request.ContentType));
        }
    }
}
