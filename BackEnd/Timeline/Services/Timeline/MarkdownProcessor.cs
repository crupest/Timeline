using Markdig;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Timeline.Controllers;

namespace Timeline.Services.Timeline
{
    public class MarkdownProcessor
    {
        public string Process(string text, Func<long, string> urlGenerator)
        {
            MarkdownDocument markdown = Markdown.Parse(text);
            foreach (var link in markdown.Descendants().Where(e => e is LinkInline).Cast<LinkInline>())
            {
                if (int.TryParse(link.Url, out var dataIndex))
                {
                    link.Url = urlGenerator(dataIndex);
                }
            }

            var writer = new StringWriter();
            NormalizeRenderer renderer = new NormalizeRenderer(writer);
            renderer.Render(markdown);

            return writer.ToString();
        }

        /// <summary>Convert data url to true url with post id.</summary>
        public string Process(string text, IUrlHelper url, string timeline, long post)
        {
            return Process(
                text,
                dataIndex => url.ActionLink(
                    nameof(TimelinePostController.DataGet),
                    nameof(TimelinePostController)[0..^nameof(Controller).Length],
                    new { timeline, post, data_index = dataIndex }
                )
            );
        }

        public byte[] Process(byte[] data, IUrlHelper url, string timeline, long post)
        {
            return Encoding.UTF8.GetBytes(Process(Encoding.UTF8.GetString(data), url, timeline, post));
        }
    }
}
