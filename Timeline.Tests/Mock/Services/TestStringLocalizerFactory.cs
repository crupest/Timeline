using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Timeline.Tests.Mock.Services
{
    internal static class TestStringLocalizerFactory
    {
        internal static IStringLocalizerFactory Create()
        {
            return new ResourceManagerStringLocalizerFactory(
                Options.Create(new LocalizationOptions()
                {
                    ResourcesPath = "Resource"
                }),
                NullLoggerFactory.Instance
            );
        }

        internal static IStringLocalizer<T> Create<T>(this IStringLocalizerFactory factory)
        {
            return new StringLocalizer<T>(factory);
        }
    }
}
