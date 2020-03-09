using System;
using static TimelineApp.Resources.Models.Validation.NicknameValidator;

namespace TimelineApp.Models.Validation
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
