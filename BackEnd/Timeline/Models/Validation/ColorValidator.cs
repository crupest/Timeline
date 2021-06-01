using System;

namespace Timeline.Models.Validation
{
    public class ColorValidator : Validator<string>
    {
        public bool PermitEmpty { get; set; } = false;
        public bool PermitDefault { get; set; } = false;
        public string DefaultValue { get; set; } = "default";

        protected override (bool, string) DoValidate(string value)
        {
            if (PermitEmpty && value.Length == 0)
            {
                return (true, GetSuccessMessage());
            }

            if (PermitDefault && value == DefaultValue)
            {
                return (true, GetSuccessMessage());
            }

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
        private ColorValidator Validator => (ColorValidator)_validator;

        public ColorAttribute() : base(typeof(ColorValidator))
        {

        }

        public bool PermitEmpty
        {
            get => Validator.PermitEmpty;
            set => Validator.PermitEmpty = value;
        }

        public bool PermitDefault
        {
            get => Validator.PermitDefault;
            set => Validator.PermitDefault = value;
        }

        public string DefaultValue
        {
            get => Validator.DefaultValue;
            set => Validator.DefaultValue = value;
        }
    }
}
