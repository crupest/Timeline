using System;
using static Timeline.Resources.Models.Validation.NicknameValidator;

namespace Timeline.Models.Validation
{
    public class NicknameValidator : Validator<string>
    {
        protected override (bool, string) DoValidate(string value)
        {
            if (value.Length > 25)
                return (false, MessageTooLong);

            return (true, GetSuccessMessage());
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class NicknameAttribute : ValidateWithAttribute
    {
        public NicknameAttribute() : base(typeof(NicknameValidator))
        {

        }
    }
}
