using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Globalization;

namespace Timeline.Tests.Mock.Services
{
    public class MockStringLocalizer : IStringLocalizer
    {
        private const string mockKey = "MOCK_KEY";
        private const string mockString = "THIS IS A MOCK LOCALIZED STRING.";

        public LocalizedString this[string name] => new LocalizedString(name, mockString);

        public LocalizedString this[string name, params object[] arguments] => new LocalizedString(name, mockString);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            yield return new LocalizedString(mockKey, mockString);
        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            return this;
        }
    }

    public class MockStringLocalizer<T> : MockStringLocalizer, IStringLocalizer<T>
    {

    }
}
