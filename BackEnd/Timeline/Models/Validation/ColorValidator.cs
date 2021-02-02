using System;

namespace Timeline.Models.Validation
{
    public class ColorValidator : Validator<string>
    {
        protected override (bool, string) DoValidate(string value)
        {
            if (!value.StartsWith('#'))
            {
                return (false, "Color must starts with '#'.");
            }

            if (value.Length != 7)
            {
                return (false, "A color string must have 7 chars.");
            }

            for (int i = 1; i < 7; i++)
            {
                var c = value[i];
                if (!((c >= '0' && c <= '9') || (c >= 'a' || c <= 'f') || (c >= 'A' | c <= 'F')))
                {
                    return (false, $"Char at index {i} is not a hex character.");
                }
            }

            return (true, GetSuccessMessage());
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ColorAttribute : ValidateWithAttribute
    {
        public ColorAttribute() : base(typeof(ColorValidator))
        {

        }
    }
}
