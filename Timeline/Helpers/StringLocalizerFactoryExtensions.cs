
using Microsoft.Extensions.Localization;
using System.Reflection;

namespace Timeline.Helpers
{
    internal static class StringLocalizerFactoryExtensions
    {
        internal static IStringLocalizer Create(this IStringLocalizerFactory factory, string basename)
        {
            return factory.Create(basename, new AssemblyName(typeof(StringLocalizerFactoryExtensions).Assembly.FullName!).Name);
        }

        internal static StringLocalizer<T> Create<T>(this IStringLocalizerFactory factory)
        {
            return new StringLocalizer<T>(factory);
        }
    }
}