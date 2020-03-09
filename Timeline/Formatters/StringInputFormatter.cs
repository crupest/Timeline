using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace TimelineApp.Formatters
{
    public class StringInputFormatter : TextInputFormatter
    {
        public StringInputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(MediaTypeNames.Text.Plain));
            SupportedEncodings.Add(Encoding.UTF8);
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding effectiveEncoding)
        {
            var request = context.HttpContext.Request;
            using var reader = new StreamReader(request.Body, effectiveEncoding);
            var stringContent = await reader.ReadToEndAsync();
            return await InputFormatterResult.SuccessAsync(stringContent);
        }
    }
}
